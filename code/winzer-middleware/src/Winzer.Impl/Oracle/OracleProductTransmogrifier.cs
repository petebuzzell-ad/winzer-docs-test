using Winzer.Library.Salsify;
using Cql.Middleware.Library.Salsify;
using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Products;
using Cql.Middleware.Library.Util;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Winzer.Library.Oracle;
using Winzer.Core.Types;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Winzer.Core.Services;

namespace Winzer.Impl.Oracle
{
    public class OracleProductTransmogrifier : IOracleProductTransmogrifier
    {
        private readonly OracleProductTransmogrifierConfiguration _config;
        private readonly ILogger _logger;
        private readonly IDictionary<string, OracleSizeComparer> _sizeComparers;
        private readonly OracleSizeComparer _defaultSizeComparer;
        private readonly IProductMapService _productMapService;
        private readonly IVariantMapService _variantMapService;

        public OracleProductTransmogrifier(OracleProductTransmogrifierConfiguration config, ILogger<OracleProductTransmogrifier> logger, IProductMapService productMapService, IVariantMapService variantMapService)
        {
            _config = config;
            _logger = logger;
            _defaultSizeComparer = new OracleSizeComparer(new List<string>());
            _sizeComparers = config.SizeSortDictionary.ToDictionary(kv => kv.Key, kv => new OracleSizeComparer(kv.Value));
            _productMapService = productMapService;
            _variantMapService = variantMapService;
        }

        public ShopifyProduct TransmogrifyToShopifyProduct(OracleCSVRecord product, BrandEnum brandId)
        {
            var rval = new ShopifyProduct();

            if (!string.IsNullOrWhiteSpace(product.ShopifyID))
            {
                rval.Id = $"gid://shopify/Product/{product.ShopifyID}";
            }

            rval.Title = product.PRODUCT_NAME;
            rval.DescriptionHtml = product.PRODUCT_DESCRIPTION;
            rval.Status = product.STATUS.ToUpper();

            //// Add Tags - Not currently covered in product import
            //var tags = new List<string>();
            //tags.AddRange(AggregateStyleNumbers(product));

            //rval.Tags = string.Join(",", tags.Distinct());

            // Get the list of options that are applicable to this product and its variants.
            rval.Options = AggregateProductOptions(product);

            AddMetafields(rval.MetaFields, product, _config.ProductMetafieldMapping, true);

            AddVariants(rval, product, brandId);

            // Make sure we put in all the images at the product level (Max 250 images for Shopify)
            var allImages = AggregateImages(product).Distinct().Take(250);
            rval.Images = allImages.Select(x => new ShopifyProductImage()
            {
                Src = x,
            }).ToList();

            // Putting the file name in the Alt text field so we can use it to match up images.
            foreach (var image in rval.Images) { image.Alt = image.JustTheFileName; }

            return rval;
        }

        private void AddVariants(ShopifyProduct shopify, OracleCSVRecord salsify, BrandEnum brandId)
        {
            var variants = salsify.Variants;
            if (variants.Any())
            {
                variants = variants.OrderBy(variant => Int32.Parse(variant.SORT_ORDER)).ToList();

                int variantPosition = 1;
                foreach (var variant in variants)
                {
                    var shopifyVariant = ConvertToShopifyVariant(shopify, variant, variantPosition, brandId);

                    // Sanity check!  Can't have multiple variants with the same option values!
                    if (shopify.Variants.Any(v => (v.Option1?.Trim() + v.Option2?.Trim() + v.Option3?.Trim()) == (shopifyVariant.Option1?.Trim() + shopifyVariant.Option2?.Trim() + shopifyVariant.Option3?.Trim())))
                    {
                        _logger.LogWarning($"Found duplicate variant {shopifyVariant.Option1} / {shopifyVariant.Option2} / {shopifyVariant.Option3} on import product {salsify.ID}");
                        continue;
                    }


                    if ((!string.IsNullOrEmpty(salsify.OPTION1_NAME) && string.IsNullOrEmpty(shopifyVariant.Option1)) ||
                        (!string.IsNullOrEmpty(salsify.OPTION2_NAME) && string.IsNullOrEmpty(shopifyVariant.Option2)) ||
                        (!string.IsNullOrEmpty(salsify.OPTION3_NAME) && string.IsNullOrEmpty(shopifyVariant.Option3)))
                    {
                        _logger.LogWarning($"Variant is missing variation option values on import product {salsify.ID}. Skipping import.");
                        continue;
                    }

                    shopify.Variants.Add(shopifyVariant);
                }

                // Final sanity check - only allow 250 variants!
                if (shopify.Variants.Count > 250)
                {
                    _logger?.LogWarning($"Salsify product {salsify.ID} has more than 250 variants, limiting to the first 250.");
                    shopify.Variants = shopify.Variants.Take(100).ToList();
                }
            } else
            {
                //standalone product - create default variant for shopify
                var shopifyVariant = ConvertToShopifyVariant(shopify, salsify, 1, brandId);
                shopify.Variants.Add(shopifyVariant);
            }
        }

        private ShopifyProductVariant ConvertToShopifyVariant(ShopifyProduct parent, OracleCSVRecord variant, int variantPosition, BrandEnum brandId)
        {
            var shopifyVariant = new ShopifyProductVariant()
            {
                SKU = variant.PRIMARY_ITEM_NUMBER,
                ProductId = parent.Id,
                Taxable = bool.Parse(variant.ISTAXABLE != "" ? variant.ISTAXABLE : "True"),
                TaxCode = variant.TAXCODE,
                Position = variantPosition++,
            };

            if (!string.IsNullOrWhiteSpace(variant.WEIGHT))
            {
                decimal weight;
                if (Decimal.TryParse(variant.WEIGHT, out weight))
                {
                    shopifyVariant.Weight = weight;
                    if (variant.WEIGHT_UNIT == "LB") shopifyVariant.WeightUnit = "POUNDS";
                    else if (variant.WEIGHT_UNIT == "OZ") shopifyVariant.WeightUnit = "OUNCES";
                }
            }

            shopifyVariant.MediaSrc = variant.IMAGE_URL;
            shopifyVariant.MediaAlt = new ShopifyProductImage() { Src = variant.IMAGE_URL }.JustTheFileName;

            if (variant.OPTION1_VALUE != null) shopifyVariant.Option1 = variant.OPTION1_VALUE;
            if (variant.OPTION2_VALUE != null) shopifyVariant.Option2 = variant.OPTION2_VALUE;
            if (variant.OPTION3_VALUE != null) shopifyVariant.Option3 = variant.OPTION3_VALUE;

            if (shopifyVariant.Metafields == null) shopifyVariant.Metafields = new List<ShopifyMetaField>();

            AddMetafields(shopifyVariant.Metafields, variant, _config.VariantMetafieldMapping, false);

            // Set default price of $1,000,000 on winzer and fastserv products to allow catalog prices to fall back as expected
            if (brandId == BrandEnum.Winzer || brandId == BrandEnum.FastServ)
            {
                shopifyVariant.Price = 1000000;
            }

            return shopifyVariant;
        }

        public int Compare(SalsifyProduct? x, SalsifyProduct? y, IList<string> outOfStockColors)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // First order by stock status, out of stock should sort to the bottom.
            var xColor = x.GetPropertyValueAsString(_config.ColorFieldName);
            var yColor = y.GetPropertyValueAsString(_config.ColorFieldName);
            var xIsOutOfStock = outOfStockColors.Contains(xColor);
            var yIsOutOfStock = outOfStockColors.Contains(yColor);

            if (xIsOutOfStock && !yIsOutOfStock)
            {
                return 1;
            }
            else if (yIsOutOfStock && !xIsOutOfStock)
            {
                return -1;
            }

            // Next sort by display order.
            var xDisplayOrder = x.GetPropertyValue<decimal?>(_config.DisplayOrderProperty);
            var yDisplayOrder = y.GetPropertyValue<decimal?>(_config.DisplayOrderProperty);
            if (xDisplayOrder < yDisplayOrder)
            {
                return -1;
            }
            else if (xDisplayOrder > yDisplayOrder)
            {
                return 1;
            }

            // Next sort by options.
            foreach ( var optionFieldName in _config.OptionFieldNames )
            {
                var comparer = _defaultSizeComparer;
                if (_sizeComparers.ContainsKey(optionFieldName))
                {
                    comparer = _sizeComparers[optionFieldName];
                }

                int comparison = comparer.Compare(x.GetPropertyValueAsString(optionFieldName), y.GetPropertyValueAsString(optionFieldName));
                if (comparison != 0) return comparison;
            }

            // Finally sort by created date.
            if (x.CreatedAt.HasValue && y.CreatedAt.HasValue)
            {
                return DateTime.Compare(x.CreatedAt.Value, y.CreatedAt.Value);
            }
            else if (x.CreatedAt.HasValue)
            {
                return -1;
            }
            else if (y.CreatedAt.HasValue)
            {
                return 1;
            }

            return 0;
        }

        private void AddMetafields(IList<ShopifyMetaField> metafields, OracleCSVRecord oracleProduct, IDictionary<string, string> mapping, bool isProductLevel)
        {
            List<string> urlFields = new List<string>();
            var multiLineFields = new List<string>();
            multiLineFields.Add("variant_description");
            var singleLineListFields = new List<string>();
            singleLineListFields.Add("warning_badges");
            singleLineListFields.Add("product_badge");
            var productReferenceFields = new List<string>();
            productReferenceFields.Add("metric_version");
            productReferenceFields.Add("imperial_version");
            var integerFields = new List<string>();
            integerFields.Add("product_lead_time");
            integerFields.Add("minimum_order_quantity");
            integerFields.Add("low_inventory_quantity");

            var websiteCategories = oracleProduct.GetPropertyValueAsString("WEBSITE_CATEGORY");
            if(!string.IsNullOrWhiteSpace(websiteCategories))
            {
                string[] categories = websiteCategories.Split('>');

                var categoriesMetafield = new ShopifyMetaField()
                {
                    Namespace = _config.ShopifyMetafieldNamespace,
                    Key = "categories",
                    Value = JsonConvert.SerializeObject(categories),
                    Type = "list.single_line_text_field"
                };
                metafields.Add(categoriesMetafield);
            }

            var attributeNames = oracleProduct.GetPropertyValueAsString("ATTTRIBUTE_NAMES");
            var attributeValues = oracleProduct.GetPropertyValueAsString("ATTRIBUTE_VALUES");
            if(!string.IsNullOrWhiteSpace(attributeNames) && !string.IsNullOrWhiteSpace(attributeValues))
            {
                string[] nameArray = attributeNames.Split('|');
                string[] valueArray = attributeValues.Split('|');
                if (nameArray.Length != valueArray.Length)
                {
                    throw new ArgumentException("The number of names and values must be the same.");
                }
                var dictionary = new Dictionary<string, string>();
                for (int i = 0; i < nameArray.Length; i++)
                {
                    dictionary[nameArray[i]] = valueArray[i];
                }
                var metafield = new ShopifyMetaField()
                {
                    Namespace = _config.ShopifyMetafieldNamespace,
                    Key = "attributes_json",
                    Value = JsonConvert.SerializeObject(dictionary, Formatting.Indented),
                    Type = "json"
                };
                metafields.Add(metafield);
            }

            foreach (var entry in mapping)
            {
                // TODO: Might need to make this more complicated to handle other data types
                // rather than just cramming everything in as a string.
                var propertyValue = oracleProduct.GetPropertyValueAsString(entry.Key);
                if (!string.IsNullOrWhiteSpace(propertyValue))
                {
                    var metafield = new ShopifyMetaField()
                    {
                        Namespace = _config.ShopifyMetafieldNamespace,
                        Key = entry.Value,
                        Value = propertyValue,
                        Type = "single_line_text_field"
                    };

                    if (singleLineListFields.Contains(entry.Value))
                    {
                        metafield.Type = "list.single_line_text_field";
                        metafield.Value = JsonConvert.SerializeObject(metafield.Value.Split(","));
                    }

                    if (integerFields.Contains(entry.Value))
                    {
                        metafield.Type = "number_integer";
                    }

                    if (propertyValue.ToLower() == "true" || propertyValue.ToLower() == "false")
                    {
                        metafield.Value = propertyValue.ToLower();
                        metafield.Type = "boolean";
                    }

                    if (propertyValue.Contains('\n') || multiLineFields.Contains(entry.Value))
                    {
                        metafield.Value = propertyValue.Replace("\\n", System.Environment.NewLine);
                        metafield.Type = "multi_line_text_field";
                    }

                    if (productReferenceFields.Contains(entry.Value))
                    {
                        metafield.Type = "product_reference";
                        continue;
                    }

                    metafields.Add(metafield);
                }
            }
        }

        private IList<ShopifyProductOption> AggregateProductOptions(OracleCSVRecord product)
        {
            var rval = new List<ShopifyProductOption>();

            if (!string.IsNullOrEmpty(product.OPTION1_NAME))
            {
                var option = new ShopifyProductOption() { Name = product.OPTION1_NAME.Trim() };
                var optionValues = product
                    .Variants
                    .Where(v => !string.IsNullOrWhiteSpace(v.OPTION1_VALUE))
                    .Select(v => v.OPTION1_VALUE.Trim())
                    .Distinct();
                option.Values = optionValues.Select(o => new ShopifyProductOptionValue()
                {
                    Name = o
                }).ToList();
                rval.Add(option);
            }

            if (!string.IsNullOrEmpty(product.OPTION2_NAME))
            {
                var option = new ShopifyProductOption() { Name = product.OPTION2_NAME.Trim() };
                var optionValues = product
                    .Variants
                    .Where(v => !string.IsNullOrWhiteSpace(v.OPTION2_VALUE))
                    .Select(v => v.OPTION2_VALUE.Trim())
                    .Distinct();
                option.Values = optionValues.Select(o => new ShopifyProductOptionValue()
                {
                    Name = o
                }).ToList();
                rval.Add(option);
            }

            if (!string.IsNullOrEmpty(product.OPTION3_NAME))
            {
                var option = new ShopifyProductOption() { Name = product.OPTION3_NAME.Trim() };
                var optionValues = product
                    .Variants
                    .Where(v => !string.IsNullOrWhiteSpace(v.OPTION3_VALUE))
                    .Select(v => v.OPTION3_VALUE.Trim())
                    .Distinct();
                option.Values = optionValues.Select(o => new ShopifyProductOptionValue()
                {
                    Name = o
                }).ToList();
                rval.Add(option);
            }

            return rval;
        }

        private IList<string> AggregateImages(OracleCSVRecord product)
        {
            var rval = new List<string>();

            if(!string.IsNullOrWhiteSpace(product.IMAGE_URL))
            {
                rval.Add(product.IMAGE_URL);
            }

            var additionalImages = product.ADDITIONAL_IMAGE_URLS;            
            if (!string.IsNullOrWhiteSpace(additionalImages))
            {
                foreach (var imageUrl in additionalImages.Split(',').Where(i => !string.IsNullOrWhiteSpace(i)))
                {
                    rval.Add(imageUrl);
                }
            }

            foreach (var variant in product.Variants)
            {
                if (!string.IsNullOrWhiteSpace(variant.IMAGE_URL) && !rval.Contains(variant.IMAGE_URL))
                {
                    rval.Add(variant.IMAGE_URL);
                }
                
                var variantAdditionalImages = variant.ADDITIONAL_IMAGE_URLS;
                if (!string.IsNullOrWhiteSpace(variantAdditionalImages))
                {
                    foreach (var variantImageUrl in variantAdditionalImages.Split(',').Where(i => !string.IsNullOrWhiteSpace(i)))
                    {
                        if (!rval.Contains(variantImageUrl))
                        {
                            rval.Add(variantImageUrl);
                        }
                    }
                }
            }

            return rval;
        }

        private IList<string> AggregateStyleNumbers(SalsifyProduct product)
        {
            var rval = new List<string>();
            var styleNo = product.GetPropertyValueAsString(_config.StyleNumberFieldName);

            if (styleNo != null)
            {
                rval.Add(styleNo);
            }
            foreach (var child in product.Children)
            {
                rval.AddRange(AggregateStyleNumbers(child));
            }

            return rval;
        }
    }

    public class OracleSizeComparer : IComparer<string?>
    {
        private readonly IList<string> _sizes;

        public OracleSizeComparer(IEnumerable<string> sizes)
        {
            _sizes = sizes.ToList();
        }

        public int Compare(string? x, string? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Now that we've established that neither value is null, see if they are both in the size array.
            if (_sizes.IndexOf(x) >= 0 && _sizes.IndexOf(y) >= 0 )
            {
                return _sizes.IndexOf(x).CompareTo(_sizes.IndexOf(y));
            }

            if (_sizes.IndexOf(x) < 0 && _sizes.IndexOf(y) < 0)
            {
                // If neither x nor y are in the array, try comparing them as numbers first, then as strings.
                decimal dx;
                decimal dy;
                if (decimal.TryParse(x, out dx) && decimal.TryParse(y, out dy))
                {
                    return dx.CompareTo(dy);
                }
                else
                {
                    return x.CompareTo(y);
                }
            }

            if (_sizes.IndexOf(x) >= 0)
            {
                // If x is in the array and y is not, then we'll consider x < y.
                return -1;
            }

            if (_sizes.IndexOf(y) >= 0)
            {
                // If y is in the array and x is not, then x > y.
                return 1;
            }

            return 0; // I don't think it's actually possible to get here..
        }
    }
}

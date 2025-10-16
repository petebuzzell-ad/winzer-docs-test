using Winzer.Library.Salsify;
using Cql.Middleware.Library.Salsify;
using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Products;
using Cql.Middleware.Library.Util;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Winzer.Impl.Salsify
{
    public class SalsifyProductTransmogrifier : ISalsifyProductTransmogrifier
    {
        private readonly SalsifyProductTransmogrifierConfiguration _config;
        private readonly ILogger _logger;
        private readonly IDictionary<string, SalsifySizeComparer> _sizeComparers;
        private readonly SalsifySizeComparer _defaultSizeComparer;

        public SalsifyProductTransmogrifier(SalsifyProductTransmogrifierConfiguration config, ILogger<SalsifyProductTransmogrifier> logger)
        {
            _config = config;
            _logger = logger;
            _defaultSizeComparer = new SalsifySizeComparer(new List<string>());
            _sizeComparers = config.SizeSortDictionary.ToDictionary(kv => kv.Key, kv => new SalsifySizeComparer(kv.Value));
        }

        public ShopifyProduct TransmogrifyToShopifyProduct(SalsifyProduct product, List<string>? addOnProducts = null)
        {
            var rval = new ShopifyProduct();

            var unformattedId = product.ShopifyId;
            if (!string.IsNullOrWhiteSpace(unformattedId))
            {
                rval.Id = $"gid://shopify/Product/{unformattedId}";
            }

            rval.Title = product.GetPropertyValueAsString(_config.ProductNameFieldName);
            rval.DescriptionHtml = product.GetPropertyValueAsString(_config.ProductDescriptionFieldName);
            rval.Status = DetermineProductStatus(product);
            rval.TemplateSuffix = product.GetPropertyValueAsString(_config.ShopifyTemplateFieldName);
            rval.Handle = product.GetPropertyValueAsString(_config.ShopifyHandleFieldName);
            rval.ProductType = product.GetPropertyValueAsString(_config.ProductTypeFieldName);

            // Add Tags
            var tags = new List<string>();
            tags.AddRange(AggregateStyleNumbers(product));

            string[]? salsifyTags = product.GetPropertyValueAsString(_config.ShopifyTagsFieldName)?.Split(',');
            if (salsifyTags != null)
            {
                foreach (var tag in salsifyTags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        tags.Add(tag.Trim());
                    }
                }
            }

            bool giftWrap = product.GetPropertyValue<bool?>(_config.GiftWrapFieldName) ?? false;
            if(!giftWrap)
            {
                tags.Add("no-wrapin");
            }

            bool isNew = GetLeafNodes(product).Any(v => v.GetPropertyValue<bool?>(_config.NewProductFieldName) == true);
            if (isNew)
            {
                tags.Add("product_inventory_new");
            }

            rval.Tags = string.Join(",", tags.Distinct());

            // Make sure we put in all the images at the product level (Max 250 images for Shopify)
            var allImages = AggregateImages(product).Distinct().Take(250);
            rval.Images = allImages.Select(x => new ShopifyProductImage()
            {
                Src = x
            }).ToList();

            // Get the list of options that are applicable to this product and its variants.
            rval.Options = AggregateProductOptions(product).Select(o => new ShopifyProductOption()
            {
                Name = o
            }).ToList();

            AddMetafields(rval.MetaFields, product, _config.ProductMetafieldMapping, true, addOnProducts);

            AddVariants(rval, product);

            AddImageSourceMetafield(rval);

            return rval;
        }

        private void AddImageSourceMetafield(ShopifyProduct product)
        {
            var data = new SalsifyImageSourceData();
            data.OriginalImageSources = product.Images
                .Select(i => i.Src ?? "").ToList();

            data.SKUToOriginalSourceMapping = product.Variants
                .Where(v => v.MediaSrc != null)
                .ToDictionary(v => v.SKU ?? "", v => v.MediaSrc ?? "");

            var serialized = JsonConvert.SerializeObject(data);
            product.MetaFields.Add(new ShopifyMetaField()
            {
                Namespace = _config.ShopifyMetafieldNamespace,
                Key = "salsify_image_sources",
                Type = "json",
                Value = serialized
            });
        }

        private void AddVariants(ShopifyProduct shopify, SalsifyProduct salsify)
        {
            var salsifyVariants = GetLeafNodes(salsify);
            salsifyVariants = SortProductVariants(salsifyVariants);
            int variantCount = salsifyVariants.Count();

            int variantPosition = 1;
            foreach ( var salsifyVariant in salsifyVariants )
            {
                // Skip the variant if Send To Shopify is false.
                if (!salsifyVariant.SendToShopify)
                    continue;

                // Also skip the variant if the variant has a parent, and the parent has Send To Shopify as false
                if (salsifyVariant.Parent != null && !salsifyVariant.Parent.SendToShopify)
                    continue;

                // Special Rule For Winzer:
                // Skip any shoes that are not "M" to reduce the number of variants if over 100.
                var shoeWidth = salsifyVariant.GetPropertyValueAsString("Shoe Width");
                if (!string.IsNullOrWhiteSpace(shoeWidth) && shoeWidth != "M" && variantCount > 100)
                    continue;

                var shopifyVariant = new ShopifyProductVariant()
                {
                    SKU = salsifyVariant.GetPropertyValueAsString(_config.SKUFieldName),
                    Price = salsifyVariant.GetPropertyValue<decimal?>(_config.PriceFieldName),
                    ProductId = shopify.Id,
                    Taxable = salsifyVariant.GetPropertyValue<bool?>(_config.IsTaxableFieldName) ?? true,
                    TaxCode = salsifyVariant.GetPropertyValueAsString(_config.TaxCodeFieldName),
                    Position = variantPosition++,
                };

                if (Decimal.TryParse(salsifyVariant.GetPropertyValueAsString(_config.WeightFieldName), out decimal weight))
                {
                    shopifyVariant.Weight = weight;
                    shopifyVariant.WeightUnit = _config.DefaultWeightUnit;
                }

                var imageUrl = salsifyVariant.GetPropertyValue<IEnumerable<string>>(_config.ProductImageFieldName)?.FirstOrDefault();
                if (imageUrl != null)
                {
                    shopifyVariant.MediaSrc = GenerateShopifyImageName(imageUrl, salsifyVariant.GetPropertyValueAsString(_config.ProductNameFieldName), salsifyVariant.GetPropertyValueAsString(_config.ColorFieldName), 0);
                }

                int optionNumber = 1;
                foreach (var option in _config.OptionFieldNames)
                {
                    var optionValue = salsifyVariant.GetPropertyValueAsString(option);
                    if (!string.IsNullOrWhiteSpace(optionValue))
                    {
                        if (optionNumber == 1)
                        {
                            shopifyVariant.Option1 = optionValue;
                        }
                        else if (optionNumber == 2)
                        {
                            shopifyVariant.Option2 = optionValue;
                        }
                        else if (optionNumber == 3)
                        {
                            shopifyVariant.Option3 = optionValue;
                        }
                        else
                        {
                            // Would also be reasonable to throw an exception here..
                        }
                        optionNumber++;
                    }
                }

                if (shopifyVariant.Metafields == null) shopifyVariant.Metafields = new List<ShopifyMetaField>();
                List<string> shopifyAddOnProducts = new List<string>();
                AddMetafields(shopifyVariant.Metafields, salsifyVariant, _config.VariantMetafieldMapping, false, shopifyAddOnProducts);

                // Sanity check!  Can't have multiple variants with the same option values!
                if (shopify.Variants.Any(v => (v.Option1 + v.Option2 + v.Option3) == (shopifyVariant.Option1 + shopifyVariant.Option2 + shopifyVariant.Option3)))
                {
                    _logger.LogWarning($"Found duplicate variant {shopifyVariant.Option1} / {shopifyVariant.Option2} / {shopifyVariant.Option3} on Salsify product {salsify.SalsifyId}");
                    continue;
                }

                shopify.Variants.Add(shopifyVariant);
            }

            // Final sanity check - only allow 100 variants!
            if (shopify.Variants.Count > 100)
            {
                _logger?.LogWarning($"Salsify product {salsify.SalsifyId} has more than 100 variants, limiting to the first 100.");
                shopify.Variants = shopify.Variants.Take(100).ToList();
            }
        }

        private IList<SalsifyProduct> SortProductVariants(IList<SalsifyProduct> salsifyVariants)
        {
            // First we make a list of all the out of stock colors.  These are colors
            // where all of the UPCs within them are out of stock.
            var outOfStockColors = salsifyVariants
                .GroupBy(v => v.GetPropertyValueAsString(_config.ColorFieldName))
                .Where(g => g.All(p => !(p.GetPropertyValue<bool?>(_config.InStockFieldName) ?? false)))
                .Select(g => g.Key).ToList();

            var sortedVariants = new List<SalsifyProduct>(salsifyVariants);
            sortedVariants.Sort(Comparer<SalsifyProduct>.Create((x, y) => Compare(x, y, outOfStockColors)));
            return sortedVariants;
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

        private IList<SalsifyProduct> GetLeafNodes(SalsifyProduct salsify)
        {
            var rval = new List<SalsifyProduct>();
            if (salsify.Children == null || !salsify.Children.Any())
            {
                rval.Add(salsify);
            }
            else
            {
                foreach (var child in salsify.Children)
                {
                    rval.AddRange(GetLeafNodes(child));
                }
            }

            return rval;
        }

        private void AddMetafields(IList<ShopifyMetaField> metafields, SalsifyProduct salsify, IDictionary<string, string> mapping, bool isProductLevel, List<string>? addOnProducts)
        {
            List<string> urlFields = new List<string>();
            urlFields.Add("swatch_image");
            urlFields.Add("featured_description_page");
            urlFields.Add("collection_link");
            urlFields.Add("image_left_image");
            urlFields.Add("image_right_image");
            urlFields.Add("image_left_image_link");
            urlFields.Add("image_right_image_link");
            urlFields.Add("image_text_link");
            urlFields.Add("image_text_media");
            urlFields.Add("split_image_left_image");
            urlFields.Add("split_image_right_image");
            urlFields.Add("split_image_right_link");
            urlFields.Add("split_image_left_link");

            var multiLineFields = new List<string>();
            multiLineFields.Add("split_image_description");
            multiLineFields.Add("featured_description");
            multiLineFields.Add("image_text_description");
            multiLineFields.Add("details_care");
            multiLineFields.Add("details_warranty");
            multiLineFields.Add("warranty_tab");
            multiLineFields.Add("care_tab");

            foreach (var entry in mapping)
            {
                // TODO: Might need to make this more complicated to handle other data types
                // rather than just cramming everything in as a string.
                var propertyValue = salsify.GetPropertyValueAsString(entry.Key);
                if (!string.IsNullOrWhiteSpace(propertyValue))
                {
                    var metafield = new ShopifyMetaField()
                    {
                        Namespace = _config.ShopifyMetafieldNamespace,
                        Key = entry.Value,
                        Value = propertyValue,
                        Type = "single_line_text_field"
                    };

                    if (urlFields.Contains(entry.Value))
                    {
                        metafield.Type = "url";
                    }

                    if (propertyValue.StartsWith("#") && propertyValue.Length == 7)
                    {
                        metafield.Type = "color";
                    }

                    if (propertyValue.ToLower() == "true" || propertyValue.ToLower() == "false")
                    {
                        metafield.Value = propertyValue.ToLower();
                        metafield.Type = "boolean";
                    }

                    if (propertyValue.Contains('\n') || multiLineFields.Contains(entry.Value))
                    {
                        metafield.Type = "multi_line_text_field";
                    }

                    metafields.Add(metafield);
                }
            }

            if (isProductLevel) {
                if (addOnProducts != null)
                {
                    var AddOnProductsMetafield = new ShopifyMetaField()
                    {
                        Namespace = _config.ShopifyMetafieldNamespace,
                        Key = "add_on_products",
                        Value = JsonConvert.SerializeObject(addOnProducts),
                        Type = "list.product_reference"
                    };

                    metafields.Add(AddOnProductsMetafield);
                }

                // Add special metafield for generic colors.
                IList<string> genericColors = new List<string>();
                foreach (var child in salsify.Children.Where(c => c.SendToShopify))
                {
                    var genericColor = child.GetPropertyValueAsString("Generic Color");
                    if (!string.IsNullOrWhiteSpace(genericColor))
                    {
                        genericColors.Add(genericColor);
                    }

                }

                var colorMetafield = new ShopifyMetaField()
                {
                    Namespace = _config.ShopifyMetafieldNamespace,
                    Key = "generic_colors",
                    Value = string.Join(",", genericColors),
                    Type = "single_line_text_field"
                };

                metafields.Add(colorMetafield);

                // Add special metafield for swatch images.
                IList<string> swatchImages = new List<string>();
                foreach (var child in salsify.Children.Where(c => c.SendToShopify))
                {
                    var swatchImage = child.GetPropertyValueAsString(_config.SwatchImageFieldName);
                    if (!string.IsNullOrWhiteSpace(swatchImage))
                    {
                        swatchImages.Add(GenerateShopifySwatchImageName(swatchImage, child.GetPropertyValueAsString(_config.ColorFieldName)));
                    }
                }

                var swatchMetafield = new ShopifyMetaField()
                {
                    Namespace = _config.ShopifyMetafieldNamespace,
                    Key = "swatches_json",
                    Value = JsonConvert.SerializeObject(swatchImages),
                    Type = "json"
                };

                metafields.Add(swatchMetafield);

                // Add a special metafield for variant inventory lifecycle codes.
                var variantList = GetLeafNodes(salsify);
                IList<object> inventoryLifecycleInfo = new List<object>();
                foreach (var variant in variantList)
                {
                    var obj = new
                    {
                        Sku = variant.SalsifyId,
                        InventoryLifecyleCode = variant.GetPropertyValueAsString(_config.InventoryLifecycleFieldName),
                        IsNew = variant.GetPropertyValue<bool?>(_config.NewProductFieldName)
                    };
                    inventoryLifecycleInfo.Add(obj);
                }

                var lifecycleMetafield = new ShopifyMetaField()
                {
                    Namespace = _config.ShopifyMetafieldNamespace,
                    Key = "variant_inventory_lifecycle_json",
                    Value = JsonConvert.SerializeObject(inventoryLifecycleInfo),
                    Type = "json"
                };

                metafields.Add(lifecycleMetafield);
            }
        }

        private string DetermineProductStatus(SalsifyProduct product)
        {
            if (product.SendToShopify)
            {
                return "ACTIVE";
            }

            return "ARCHIVED";
        }

        private IList<string> AggregateProductOptions(SalsifyProduct product)
        {
            // Gotta do this in a somewhat strange way to make sure the returned list of options
            // contains all options for all leaf nodes, without duplicates, and preserving the order
            // specified in OptionFieldNames.  Can't just use .Distinct since it doesn't guarentee order is preserved.
            var rval = new List<string>();
            foreach ( var optionField in _config.OptionFieldNames)
            {
                if (rval.Contains(optionField)) continue;
                if (product[optionField] != null) rval.Add(optionField);
            }

            foreach (var child in product.Children)
            {
                var childOptions = AggregateProductOptions(child);
                foreach ( var childOption in childOptions)
                {
                    if (!rval.Contains(childOption)) rval.Add(childOption);
                }
            }

            return rval;
        }

        private IList<string> AggregateImages(SalsifyProduct product)
        {
            var rval = new List<string>();

            if (product.SendToShopify)
            {
                var images = product.GetPropertyValue<IEnumerable<string>>(_config.ProductImageFieldName);
                if (images != null && images.Any())
                {
                    int position = 0;
                    foreach (var image in images.Where(i => !string.IsNullOrWhiteSpace(i)))
                    {
                        var url = GenerateShopifyImageName(image, product.GetPropertyValueAsString(_config.ProductNameFieldName), product.GetPropertyValueAsString(_config.ColorFieldName), position);
                        rval.Add(url);
                        position++;
                    }
                }

                foreach (var child in product.Children)
                {
                    rval.AddRange(AggregateImages(child));
                }
            }

            return rval;
        }

        private string GenerateShopifyImageName(string originalSrc, string productName, string? color, int position)
        {
            var baseImageUrl = "https://images.salsify.com/images/w_900,h_900";
            var imageId = Regex.Match(originalSrc, @"/upload/.*?/([a-zA-Z0-9]*)\.", RegexOptions.Compiled);
            var productNameSlug = Slugifier.Slugify(productName);

            if (!string.IsNullOrEmpty(color))
            {
                var colorSlug = Slugifier.Slugify(color);
                return $"{baseImageUrl}/{imageId.Groups[1].Captures[0].Value}/{productNameSlug}__{colorSlug}_{position}.jpg";
            }
            else
            {
                return $"{baseImageUrl}/{imageId.Groups[1].Captures[0].Value}/{productNameSlug}__{position}.jpg";
            }
        }

        private string GenerateShopifySwatchImageName(string originalSrc, string color)
        {
            var baseImageUrl = "https://images.salsify.com/images";
            var imageId = Regex.Match(originalSrc, @"/upload/.*?/([a-zA-Z0-9]*)\.", RegexOptions.Compiled);

            var colorSlug = Slugifier.Slugify(color);
            return $"{baseImageUrl}/{imageId.Groups[1].Captures[0].Value}/{colorSlug}.jpg";
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

    public class SalsifySizeComparer : IComparer<string?>
    {
        private readonly IList<string> _sizes;

        public SalsifySizeComparer(IEnumerable<string> sizes)
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

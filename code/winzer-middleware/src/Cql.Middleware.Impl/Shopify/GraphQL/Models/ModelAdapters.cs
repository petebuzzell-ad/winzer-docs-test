using Cql.Middleware.Impl.Shopify.Services;
using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Products;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Impl.Shopify.GraphQL.Models
{
    public class ShopifyGraphQLProductUpdateInputAdapter : ShopifyGraphQLProductUpdateInput
    {
        public ShopifyGraphQLProductUpdateInputAdapter(ShopifyProduct product)
        {
            id = product.Id;
            handle = product.Handle;
            productType = product.ProductType;
            title = product.Title;
            descriptionHtml = product.DescriptionHtml;
            status = product.Status;
            vendor = product.Vendor;
            templateSuffix = product.TemplateSuffix;
            tags = product.Tags;
            //published = product.Status == "ACTIVE" ? true : false;

            if (product.MetaFields != null && product.MetaFields.Any())
            {
                metafields = product.MetaFields.Select(m => new ShopifyGraphQLMetafieldInputAdapter(m)).ToArray();
            }
        }
    }

    public class ShopifyGraphQLProductInputAdapter : ShopifyGraphQLProductInput
    {
        public ShopifyGraphQLProductInputAdapter(ShopifyProduct product)
        {
            id = product.Id;
            handle = product.Handle;
            productType = product.ProductType;
            title = product.Title;
            descriptionHtml = product.DescriptionHtml;
            giftCard = false;
            status = product.Status;
            vendor = product.Vendor;
            templateSuffix = product.TemplateSuffix;
            tags = product.Tags;
            published = product.Status == "ACTIVE" ? true : false;
                
            if (product.MetaFields != null && product.MetaFields.Any())
            {
                metafields = product.MetaFields.Select(m => new ShopifyGraphQLMetafieldInputAdapter(m)).ToArray();
            }

            if (product.Options  != null && product.Options.Any())
            {
                var options = new List<ShopifyGraphQLOptionCreateInput>();
                int i = 0;
                foreach ( var option in product.Options )
                {
                    var newOption = new ShopifyGraphQLOptionCreateInput()
                    {
                        name = option.Name,
                        position = ++i,
                    };

                    var optionValues = new List<string>();
                    switch (i)
                    {
                        case 1:
                            var tmp = product.Variants?.Select(v => v.Option1?.Trim())?.Where(o => !string.IsNullOrWhiteSpace(o));
                            if (tmp != null) optionValues.AddRange(tmp);
                            break;
                        case 2:
                            tmp = product.Variants?.Select(v => v.Option2?.Trim())?.Where(o => !string.IsNullOrWhiteSpace(o));
                            if (tmp != null) optionValues.AddRange(tmp);
                            break;
                        case 3:
                            tmp = product.Variants?.Select(v => v.Option3?.Trim())?.Where(o => !string.IsNullOrWhiteSpace(o));
                            if (tmp != null) optionValues.AddRange(tmp);
                            break;
                        default:
                            continue; // Only 3 options are supported.
                    }
                    newOption.values = optionValues.Distinct().Select(v => new ShopifyGraphQLProductOptionValueCreateInput() { name = v }).ToArray();
                    if (newOption.values.Any())
                    {
                        options.Add(newOption);
                    }
                }
                productOptions = options.ToArray();
            }
        }
    }

    public class ShopifyGraphQLProductVariantsBulkInputAdapter : ShopifyGraphQLProductVariantBulkInput
    {
        public ShopifyGraphQLProductVariantsBulkInputAdapter(ShopifyProduct product, ShopifyProductVariant variant)
        {
            id = variant.Id;

            barcode = variant.Barcode;
            compareAtPrice = variant.CompareAtPrice;
            price = variant.Price;
            taxable = variant.Taxable;
            taxCode = variant.TaxCode;
            mediaId = variant.MediaId;
            mediaSrc = variant.MediaSrc;

            var tmp = new List<ShopifyGraphQLVariantOptionValueInput>();
            if (!string.IsNullOrWhiteSpace(variant.Option1) && product.Options.Count() > 0)
            {
                tmp.Add(new ShopifyGraphQLVariantOptionValueInput() { optionName = product.Options.First().Name, name = variant.Option1 });
            }

            if (!string.IsNullOrWhiteSpace(variant.Option2) && product.Options.Count() > 1)
            {
                tmp.Add(new ShopifyGraphQLVariantOptionValueInput() { optionName = product.Options.Skip(1).First().Name, name = variant.Option2 });
            }

            if (!string.IsNullOrWhiteSpace(variant.Option3) && product.Options.Count() > 2)
            {
                tmp.Add(new ShopifyGraphQLVariantOptionValueInput() { optionName = product.Options.Skip(2).First().Name, name = variant.Option3 });
            }

            optionValues = tmp.ToArray();

            inventoryItem = new ShopifyGraphQLInventoryItemInput()
            {
                sku = variant.SKU,
                tracked = true,
                measurement = new ShopifyGraphQLInventoryMeasurementInput()
                {
                    weight = new ShopifyGraphQLWeightInput() { value = variant.Weight, unit = variant.WeightUnit }
                }
            };


            metafields = variant.Metafields?.Select(m => new ShopifyGraphQLMetafieldInputAdapter(m)).ToArray();
            //position = variant.Position;
        }
    }
/*
    public class ShopifyGraphQLProductVariantInputAdapter : ShopifyGraphQLProductVariantInput
    {
        public ShopifyGraphQLProductVariantInputAdapter(ShopifyProductVariant variant, bool includeImage, bool onlyImage)
        {
            id = variant.Id;
            mediaSrc = includeImage ? (variant.MediaId == null ? variant.MediaSrc : null) : null;
            mediaId = variant.MediaId;

            if (!onlyImage)
            {
                this.productId = variant.ProductId;
                barcode = variant.Barcode;
                compareAtPrice = variant.CompareAtPrice;
                options = new string[] { variant.Option1, variant.Option2, variant.Option3 }.Where(o => o != null).ToArray();
                position = variant.Position;
                price = variant.Price;
                sku = variant.SKU;
                taxable = variant.Taxable;
                taxCode = variant.TaxCode;
                weight = variant.Weight;
                weightUnit = variant.WeightUnit;
                requiresShipping = variant.RequiresShipping;
                fulfillmentServiceId = variant.FulfillmentService;
                metafields = variant.Metafields?.Select(m => new ShopifyGraphQLMetafieldInputAdapter(m)).ToArray();
                inventoryItem = new ShopifyGraphQLInventoryItemInput() { tracked = true };
            }
        }
    }
*/
    public class ShopifyGraphQLMetafieldInputAdapter : ShopifyGraphQLMetafieldInput
    {
        public ShopifyGraphQLMetafieldInputAdapter(ShopifyMetaField metafield)
        {
            Namespace = metafield.Namespace;
            description = metafield.Description;
            id = metafield.Id;
            key = metafield.Key;
            Type = metafield.Type;
            value = metafield.Value;
        }
    }
}

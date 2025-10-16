using Winzer.Library.Salsify;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winzer.Impl.Salsify
{
    public class SalsifyExportParser : ISalsifyExportParser
    {
        private readonly ILogger _logger;

        public SalsifyExportParser(ILogger<SalsifyExportParser> logger)
        {
            _logger = logger;
        }

        public IEnumerable<SalsifyProduct> ParseSalsifyJsonExport(string json) {
            // The actual export from salsify is an array of objects each of which contains a part of what we want,
            // so gotta do this funny business to make it make sense.
            var rawExportData = JsonConvert.DeserializeObject<SalsifyJsonExport[]>(json);
            var theRealData = new SalsifyJsonExport();
            foreach (var item in rawExportData)
            {
                if (item.header != null) theRealData.header = item.header;
                if (item.attributes != null) theRealData.attributes = item.attributes;
                if (item.digital_assets != null) theRealData.digital_assets = item.digital_assets;
                if (item.products != null) theRealData.products = item.products;
            }

            return BuildSalsifyProducts(theRealData);
        }

        public async Task<IEnumerable<SalsifyProduct>> ParseSalsifyJsonExport(Uri exportUri)
        {
            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync(exportUri);
            return ParseSalsifyJsonExport(json);
        }

        public IEnumerable<SalsifyProduct> BuildSalsifyProducts(SalsifyJsonExport rawExportData)
        {
            if (rawExportData == null || rawExportData.products == null || !rawExportData.products.Any())
            {
                return Enumerable.Empty<SalsifyProduct>();
            }

            // Build up a few indexes that we'll use for lookups as we build the products and store them in a context object
            // that we can easily pass around.
            var context = new SalsifyExportParserContext
            {
                AttributeIndex = rawExportData.attributes.ToDictionary(a => a.SalsifyId ?? "", a => a),
                AssetIndex = rawExportData.digital_assets.ToDictionary(a => a.SalsifyId ?? "", a => a.SalsifyUrl ?? ""),
                ProductIndex = rawExportData.products.ToDictionary(a => a.ProductID ?? "", a => a)
            };

            foreach (var product in rawExportData.products)
            {
                BuildSalsifyProduct(product, context);
            }

            return context.AssembledProducts.Values.Where(a => a.Parent == null);
        }

        private void BuildSalsifyProduct(SalsifyExportProduct exportProduct, SalsifyExportParserContext context)
        {
            // If we've already built the product, just bail.
            if (context.AssembledProducts.ContainsKey(exportProduct.ProductID)) return;

            if (string.IsNullOrWhiteSpace(exportProduct.ParentId))
            {
                // If this is a top level product with no parent, we can just build it now.
                var product = new SalsifyProduct();
                PopulateProductProperties(product, exportProduct, context);
                context.AssembledProducts.Add(exportProduct.ProductID, product);
            }
            else if (context.AssembledProducts.ContainsKey(exportProduct.ParentId))
            {
                // If this is a child product and we've already built the parent...
                var parent = context.AssembledProducts[exportProduct.ParentId];
                var product = new SalsifyProduct(parent);
                PopulateProductProperties(product, exportProduct, context);
                parent.Children.Add(product);
                context.AssembledProducts.Add(exportProduct.ProductID, product);
            }
            else
            {
                // This is a child product and we haven't built the parent yet.
                // Need to build the parent before we build the child.
                if (!context.ProductIndex.ContainsKey(exportProduct.ParentId))
                {
                    // If we can't find the parent product in the export, skip this product since we most likely won't have complete data for it
                    // without the data that's inherited from the parent product.
                    _logger.LogError("Found a child product in the export without its parent!  Product ID was {0} and expected parent id was {1}", exportProduct.ProductID, exportProduct.ParentId);
                    context.ErrorsOccurred = true;
                    return;
                }

                var parentExportProduct = context.ProductIndex[exportProduct.ParentId];
                BuildSalsifyProduct(parentExportProduct, context);
                if (context.AssembledProducts.ContainsKey(exportProduct.ParentId))
                {
                    var parent = context.AssembledProducts[exportProduct.ParentId];
                    var product = new SalsifyProduct(parent);
                    PopulateProductProperties(product, exportProduct, context);
                    parent.Children.Add(product);
                    context.AssembledProducts.Add(exportProduct.ProductID, product);
                }
            }
        }

        private void PopulateProductProperties(SalsifyProduct product, SalsifyExportProduct exportProduct, SalsifyExportParserContext context)
        {
            product["ProductID"] = exportProduct.ProductID;
            product.ParentId = exportProduct.ParentId;
            product.CreatedAt = exportProduct.CreatedAt;
            product.UpdatedAt = exportProduct.UpdatedAt;
            product.DataInheritanceHierarchyLevelId = exportProduct.SalsifyDataInheritanceHierarchyLevelId;
            product.SalsifyId = exportProduct.SalsifyId;
            product.ShopifyId = exportProduct.ShopifyProductId;
            product.ShopifyHash = exportProduct.ShopifyProductHash;
            product.TodayDate = exportProduct.TodayDate;

            // Handle other properties that are going to vary by Salsify implementation.
            foreach ( var entry in exportProduct.Attributes)
            {
                var attributeName = entry.Key;
                var token = entry.Value;

                // Skip any other special "salsify:" attributes..
                if (attributeName.StartsWith("salsify:")) continue;

                if (!context.AttributeIndex.ContainsKey(attributeName))
                {
                    _logger.LogError("Expected to find attribute data for property {0}, but it was missing from the file!", attributeName);
                    context.ErrorsOccurred = true;
                    continue;
                }

                var attribute = context.AttributeIndex[attributeName];
                switch (attribute.SalsifyDataType)
                {
                    case "digital_asset":
                        WithTokenAsArray<string>(token, assetIds =>
                        {
                            var assetUrls = new List<string>();
                            foreach (var assetId in assetIds)
                            {
                                if (context.AssetIndex.ContainsKey(assetId))
                                {
                                    var assetUrl = context.AssetIndex[assetId];
                                    if (!string.IsNullOrWhiteSpace(assetUrl))
                                    {
                                        assetUrls.Add(context.AssetIndex[assetId]);
                                    }
                                }
                                else
                                {
                                    _logger.LogError("Expected to find digital asset with ID {0} in export, but it wasn't there..", assetId);
                                    context.ErrorsOccurred = true;
                                }
                            }
                            product[attributeName] = assetUrls;
                        });
                        break;
                    case "quantified_product":
                        WithTokenAsArray<SalsifyExportIncludedProduct>(token, products =>
                        {
                            product[attributeName] = products.Select(p => new QuantifiedProductReference() { ProductId = p.SalsifyProductId ?? "", Qty = p.SalsifyQuantity.GetValueOrDefault(0) }).ToList();
                        });
                        break;
                    case "boolean":
                        if (token.Type == JTokenType.Boolean)
                        {
                            product[attributeName] = token.ToObject<bool>();
                        }
                        else
                        {
                            _logger.LogError("Expected a boolean property to be boolean in the JSON, but wasn't.  Property was {0} and value was {1}", attributeName, token.ToString());
                            context.ErrorsOccurred = true;
                        }
                        break;
                    case "number":
                        WithTokenAsArray<decimal?>(token, val =>
                        {
                            product[attributeName] = val.Count > 1 ? val : val.First();
                        });
                        break;
                    default:
                        WithTokenAsArray<string>(token, val =>
                        {
                            product[attributeName] = val.Count > 1 ?
                                val.Select(v => v.Trim()).ToList() :
                                val.First().Trim();
                        });
                        break;
                }
            }
        }

        private static void WithTokenAsArray<T>(JToken token, Action<List<T>> func)
        {
            if (token.Type == JTokenType.Array)
            {
                var arr = token.ToObject<List<T>>();
                func(arr);
            }
            else
            {
                var arr = new List<T>() { token.ToObject<T>() };
                func(arr);
            }
        }
    }

    /// <summary>
    /// Allows me to pass a bunch of things to various methods in the parser without having tons of parameters to deal with.
    /// </summary>
    class SalsifyExportParserContext
    {
        public IDictionary<string, SalsifyExportAttribute> AttributeIndex { get; set; } = new Dictionary<string, SalsifyExportAttribute>();

        public IDictionary<string, string> AssetIndex { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, SalsifyExportProduct> ProductIndex { get; set; }  = new Dictionary<string, SalsifyExportProduct>();

        public IDictionary<string, SalsifyProduct> AssembledProducts { get; set; } = new Dictionary<string, SalsifyProduct>();

        public bool ErrorsOccurred { get; set; } = false;
    }
}

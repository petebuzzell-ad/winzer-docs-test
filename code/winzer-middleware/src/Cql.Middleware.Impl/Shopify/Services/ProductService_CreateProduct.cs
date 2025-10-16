using Cql.Middleware.Impl.Shopify.GraphQL.Models;
using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Products;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Dasync.Collections;

namespace Cql.Middleware.Impl.Shopify.Services
{
    public partial class ProductService
    {
        public async Task<ShopifyCreateProductResult> CreateProduct(ShopifyProduct product)
        {
            // If it's a big product, we need to create it in pieces.
            // Trying to create it all at once will result in a timeout error in Shopify.  Even worse,
            // even though you get a timeout error, the product will (likely, eventually) be created anyways.
            _logger.LogDebug("Creating bare product");
            var rval = await CreateProduct_Internal(product);
            product.Id = rval.ProductId;
            foreach ( var variant in product.Variants)
            {
                variant.ProductId = product.Id;
            }

            _logger.LogDebug("Creating variants & images");
            var remainingVariantSKUtoIDMap = await CreateProduct_AddNewVariants(product);
            foreach (var entry in remainingVariantSKUtoIDMap) rval.VariantSKUToIdMap.Add(entry);

            foreach ( var variant in product.Variants )
            {
                variant.Id = rval.VariantSKUToIdMap[variant.SKU];
            }

            _logger.LogDebug("Adding images to product");
            await CreateProduct_AddImages(product);
            return rval;
        }

        private async Task<ShopifyCreateProductResult> CreateProduct_Internal(ShopifyProduct product)
        {
            var input = new ShopifyGraphQLProductInputAdapter(product);

            var query = @"
mutation productCreate($input: ProductInput!) {
    productCreate(input: $input) {
        product {
            id
            handle
        }
        userErrors {
            field
            message
        }
    }
}
";

            var parameters = new
            {
                input = input,
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductCreateResult>(query, parameters, true);
            if (result.IsSuccessful())
            {
                return new ShopifyCreateProductResult()
                {
                    ProductId = result.productCreate.product.Id ?? "",
                    Handle = result.productCreate.product.handle
                };
            }

            var errors = result.ErrorList();
            if (errors.Any())
            {
                var firstError = errors.First();
                throw new Exception($"Product creation for product '{input.title}' was unsuccessful.  First error was {string.Join(".",firstError.field ?? new string[] { "(no field)"})}: {firstError.message}.  Note that due to Shopify bugs, Shopify may have created the product anyways.");
            }
            else
            {
                throw new Exception($"Product creation for product '{input.title}' was unsuccessful.  An unexpected or invalid response received from Shopify.");
            }
        }
        private async Task CreateProduct_AddImages(ShopifyProduct product)
        {
            // TODO: Only add images that aren't associated to a variant.
            // Run image adds individually (paralleized)
            var imageSourceToImageIdMap = new ConcurrentDictionary<string, string>();
            if (product.Images != null && product.Images.Any())
            {
                var imagesToAdd = product.Images.Where(i => !product.Variants.Any(v => v.MediaSrc == i.Src));
                var imageMedia = imagesToAdd.Select(i => new ShopifyGraphQLProductCreateMediaInput()
                {
                    originalSource = i.Src,
                    mediaContentType = "IMAGE",
                    alt = i.Alt
                });

                var result = await CreateProduct_AddMediaBatch(product.Id, imageMedia.ToArray());
            }
        }

        private async Task<ShopifyGraphQLMedia[]> CreateProduct_AddMediaBatch(string productId, ShopifyGraphQLProductCreateMediaInput[] input)
        {

            var query = @"
mutation productCreateMedia($media: [CreateMediaInput!]!, $productId: ID!) {
    productCreateMedia(media: $media, productId: $productId) {
        media {
            id
        }
        userErrors {
            field
            message
        }
    }
}
";

            var parameters = new
            {
                productId,
                media = input,
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductCreateMediaResult>(query, parameters, true);
            if (result.IsSuccessful())
            {
                return result.productCreateMedia.media;
            }

            var errors = result.ErrorList();
            if (errors.Any())
            {
                var firstError = errors.First();
                throw new Exception($"Adding images to product '{productId}' was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message} ({firstError.code}).  Note that the product was still created.  This will require manual intervention to repair.");
            }
            else
            {
                throw new Exception($"Adding images to product '{productId}' was unsuccessful.  An unexpected or invalid response received from Shopify.  Note that the product was still created.  This will require manual intervention to repair.");
            }

        }

        private async Task<IDictionary<string, string>> CreateProduct_AddNewVariants(ShopifyProduct product)
        {
            var newVariants = product.Variants.Where(v => string.IsNullOrWhiteSpace(v.Id)).Select(v => new ShopifyGraphQLProductVariantsBulkInputAdapter(product, v));

            // Log detailed variant data for debugging
            _logger.LogInformation($"Creating {newVariants.Count()} variants for product {product.Id}");
            foreach (var variant in newVariants)
            {
                _logger.LogInformation($"Variant SKU: {variant.inventoryItem?.sku}, Price: {variant.price}, CompareAtPrice: {variant.compareAtPrice}, Barcode: {variant.barcode}");
                _logger.LogInformation($"Option Values: {string.Join(", ", variant.optionValues?.Select(ov => $"{ov.optionName}={ov.name}") ?? new string[0])}");
                _logger.LogInformation($"Weight: {variant.inventoryItem?.measurement?.weight?.value} {variant.inventoryItem?.measurement?.weight?.unit}");
                _logger.LogInformation($"Metafields Count: {variant.metafields?.Length ?? 0}");
            }

            // Validate variant data before sending to Shopify
            var validVariants = new List<ShopifyGraphQLProductVariantsBulkInputAdapter>();
            foreach (var variant in newVariants)
            {
                var validationErrors = new List<string>();
                
                // Check required fields
                if (string.IsNullOrWhiteSpace(variant.inventoryItem?.sku))
                    validationErrors.Add("SKU is required");
                
                if (variant.price == null || variant.price <= 0)
                    validationErrors.Add("Price must be greater than 0");
                
                // Check option values match product options
                if (variant.optionValues != null)
                {
                    var productOptionNames = product.Options.Select(o => o.Name).ToHashSet();
                    foreach (var optionValue in variant.optionValues)
                    {
                        if (!productOptionNames.Contains(optionValue.optionName))
                            validationErrors.Add($"Option '{optionValue.optionName}' does not exist on product");
                        
                        if (string.IsNullOrWhiteSpace(optionValue.name))
                            validationErrors.Add($"Option value for '{optionValue.optionName}' cannot be empty");
                    }
                }
                
                // Check weight data
                if (variant.inventoryItem?.measurement?.weight != null)
                {
                    if (variant.inventoryItem.measurement.weight.value < 0)
                        validationErrors.Add("Weight cannot be negative");
                    
                    var validUnits = new[] { "POUNDS", "OUNCES", "KILOGRAMS", "GRAMS" };
                    if (!validUnits.Contains(variant.inventoryItem.measurement.weight.unit))
                        validationErrors.Add($"Invalid weight unit: {variant.inventoryItem.measurement.weight.unit}");
                }
                
                if (validationErrors.Any())
                {
                    _logger.LogError($"Variant validation failed for SKU {variant.inventoryItem?.sku}: {string.Join(", ", validationErrors)}");
                    continue;
                }
                
                validVariants.Add(variant);
            }
            
            if (!validVariants.Any())
            {
                throw new Exception($"All variants failed validation for product {product.Id}");
            }
            
            if (validVariants.Count != newVariants.Count())
            {
                _logger.LogWarning($"Filtered out {newVariants.Count() - validVariants.Count()} invalid variants for product {product.Id}");
            }

            var query = @"
mutation productVariantsBulkCreate($productId: ID!, $variants: [ProductVariantsBulkInput!]!, $strategy: ProductVariantsBulkCreateStrategy) {
    productVariantsBulkCreate(variants: $variants, productId: $productId, strategy: $strategy) {
        productVariants {
            id
            sku
        }
        userErrors {
            field
            message
        }
    }
}
";
            var rval = new Dictionary<string, string>();

            var parameters = new
            {
                productId = product.Id,
                strategy = "REMOVE_STANDALONE_VARIANT",
                variants = validVariants
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductVariantBulkCreateResult>(query, parameters, false);
            if (result.IsSuccessful())
            {
                foreach ( var variant in result.productVariantsBulkCreate.productVariants )
                {
                    rval.TryAdd(variant.sku, variant.id);
                }

                foreach ( var variant in newVariants )
                {
                    if (!rval.ContainsKey(variant.inventoryItem.sku))
                    {
                        throw new Exception($"Was supposed to create variant with sku {variant.inventoryItem.sku} but it wasn't created and no error was returned!");
                    }
                }
            }
            else
            {
                var errors = result.ErrorList();
                if (errors.Any())
                {
                    var errorDetails = string.Join("; ", errors.Select(e => $"{string.Join(".", e.field ?? new string[] { "(unknown field)" })}: {e.message}"));
                    _logger.LogError($"Shopify variant creation errors for product {product.Id}: {errorDetails}");
                    
                    var firstError = errors.First();
                    throw new Exception($"Product variant creation for product {product.Id} was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(unknown field)" })}: {firstError.message}.  All errors: {errorDetails}.  Note that due to Shopify bugs, Shopify may have created the variant anyways.  This will require manual intervention to correct.");
                }
                else
                {
                    _logger.LogError($"Shopify variant creation failed for product {product.Id} with no specific error details. Response: {result.RawResponse}");
                    throw new Exception($"Product variant creation for product {product.Id} was unsuccessful.  An unexpected or invalid response received from Shopify.  This will require manual intervention to correct.");
                }
            }

            return rval;
        }
    }
}

using Cql.Middleware.Impl.Shopify.GraphQL.Models;
using Cql.Middleware.Library.Shopify.Products;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Impl.Shopify.Services
{
    public partial class ProductService
    {

        public async Task UpdateProductVariantPrices(string productId, IDictionary<string,decimal> variantPrices)
        {
            if (!productId.StartsWith("gid"))
            {
                productId = $"gid://shopify/Product/{productId}";
            }

            var query = @"
mutation productVariantsBulkUpdate($productId: ID!, $variants: [ProductVariantsBulkInput!]!) {
    productVariantsBulkUpdate(productId: $productId, variants: $variants) {
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

            var parameters = new
            {
                productId = productId,
                variants = variantPrices.Select(e =>
                    new ShopifyGraphQLProductVariantBulkInput()
                    {
                        price = e.Value,
                        id = e.Key.StartsWith("gid") ? e.Key : $"gid://shopify/ProductVariant/{e.Key}",
                    }
                )
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductVariantBulkUpdateResult>(query, parameters, true);
            if (!result.IsSuccessful())
            {
                var errors = result.ErrorList();
                if (errors.Any())
                {
                    var firstError = errors.First();
                    throw new Exception($"Product pricing update was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message}.  Note that due to Shopify bugs, Shopify may have updated the product anyways.");
                }
                else
                {
                    throw new Exception("Product pricing update was unsuccessful.  An unexpected or invalid response received from Shopify.");
                }
            }
        }

        public async Task UpdateProductStatus(string productId, string status)
        {
            if (!productId.StartsWith("gid"))
            {
                productId = $"gid://shopify/Product/{productId}";
            }

            var query = @"
mutation productChangeStatus($productId: ID!, $status: ProductStatus!) {
    productChangeStatus(productId: $productId, status: $status) {
        product {
            id
        }
        userErrors {
            field
            message
        }
    }
}";

            var parameters = new
            {
                productId = productId,
                status = status
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductChangeStatusResult>(query, parameters, true);
            if (!result.IsSuccessful())
            {
                var errors = result.ErrorList();
                if (errors.Any())
                {
                    var firstError = errors.First();
                    throw new Exception($"Product status update was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message}.  Note that due to Shopify bugs, Shopify may have updated the product anyways.");
                }
                else
                {
                    throw new Exception("Product status update was unsuccessful.  An unexpected or invalid response received from Shopify.");
                }
            }
        }
        public async Task<ShopifyUpdateProductResult> UpdateProduct(ShopifyProductWithDeletes product)
        {
            await UpdateProduct_Data(product);

            await UpdateProduct_Options(product);

            var variantSKUToIdMap = await UpdateProduct_Variants(product);

            await UpdateProduct_Images(product); 

            await UpdateProduct_RemoveMetafields(product);

            return new ShopifyUpdateProductResult() { NewVariantsSKUToIdMap = variantSKUToIdMap };
        }

        private async Task UpdateProduct_Options(ShopifyProductWithDeletes product)
        {
            if (product.OptionUpdates.Any())
            {
                foreach (var optionUpdate in product.OptionUpdates)
                {
                    if (optionUpdate.OptionId != null)
                    {
                        _logger.LogInformation($"Updating option values for option '{optionUpdate.OptionName}' on product {product.Id}");
                        await UpdateProduct_UpdateOptionValues(product.Id, optionUpdate);
                    }
                    else
                    {
                        _logger.LogInformation($"Adding new option '{optionUpdate.OptionName}' on product {product.Id}");
                        await UpdateProduct_AddNewOption(product.Id, optionUpdate);
                    }
                }
            }
        }

        private async Task UpdateProduct_AddNewOption(string productId, ShopifyProductOptionUpdate optionUpdate)
        {
            var query = @"
mutation createOption($productId: ID!, $options: [OptionCreateInput!]!) {
    productOptionsCreate(productId: $productId, options: $options) {
        userErrors {
            field
            message
            code
        }
    }
}
";
            var input = new
            {
                productId = productId,
                options = new ShopifyGraphQLOptionCreateInput[]
                {
                    new ShopifyGraphQLOptionCreateInput()
                    {
                        name = optionUpdate.OptionName,
                        values = optionUpdate.OptionValuesToAdd.Select(v => new ShopifyGraphQLProductOptionValueCreateInput()
                        {
                            name = v
                        }).ToArray()
                    }
                }
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductOptionsCreateResult>(query, input, false);
            if (!result.IsSuccessful())
            {
                var firstError = result.ErrorList().First();
                throw new Exception($"Product option creation was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message}.");
            }
        }

        private async Task UpdateProduct_UpdateOptionValues(string productId, ShopifyProductOptionUpdate optionUpdate)
        {
            var query = @"
mutation updateOption($productId: ID!, $option: OptionUpdateInput!, $optionValuesToAdd: [OptionValueCreateInput!]) {
    productOptionUpdate(productId: $productId, option: $option, optionValuesToAdd: $optionValuesToAdd) {
        userErrors {
            field
            message
            code
        }
    }
}
";
            var input = new
            {
                productId = productId,
                option = new
                {
                    id = optionUpdate.OptionId,
                    name = optionUpdate.OptionName,
                },
                optionValuesToAdd = optionUpdate.OptionValuesToAdd.Select(v => new
                {
                    name = v
                })
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductOptionUpdateResult>(query, input, true);
            if (!result.IsSuccessful())
            {
                var firstError = result.ErrorList().First();
                throw new Exception($"Product option update was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message}.");
            }
        }

        private async Task UpdateProduct_Images(ShopifyProductWithDeletes product)
        {
            // Find any new images.
            var newImages = product.Images.Where(i => !string.IsNullOrWhiteSpace(i.Src) && string.IsNullOrWhiteSpace(i.MediaId));
            if (newImages.Any()) _logger.LogInformation($"Adding {newImages.Count()} new images to product {product.Id}..");
            foreach (var image in newImages)
            {
                var variantsWithThisImage = product.Variants.Where(v => image.IsSameImageAs(new ShopifyProductImage() { MediaId = v.MediaId, Src = v.MediaSrc }));

                // This process is split into two parts, one that adds the product images that aren't associated to any
                // variants and the other that adds the images that are associated to a variant.  This needs to be done
                // because you can't associate an image to a variant if that image is still being "processed" by Shopify
                // (which it almost certainly will be since we just added it)
                if (!variantsWithThisImage.Any())
                {
                    // Only add directly to the product if this image is not also associated to a variant.
                    _logger.LogInformation($"Adding image {image.Src} to product {product.Id}");
                    var newImageId = await UpdateProduct_AddImage(product.Id, image);
                }
                else
                {
                    // Add any new images that have variant associations to the variants directly.
                    _logger.LogInformation($"Adding image  {image.Src} to {variantsWithThisImage.Count()} variants on product {product.Id}");
                    await UpdateProduct_AddNewImageToVariants(product.Id, variantsWithThisImage, image);
                }
            }

            // Update any existing images that have changed.  Actually just delete and re-create them since we can't actually just update the image source
            // anymore after productImageUpdate was deprecated.
            // Note that this depends on the shopify image urls containing the word "shopify" in them somewhere.
            var changedImages = product.Images.Where(i => !string.IsNullOrWhiteSpace(i.Src) && !string.IsNullOrWhiteSpace(i.MediaId) && !i.Src.Contains("shopify"));
            if (changedImages.Any()) _logger.LogInformation($"Updating {changedImages.Count()} images on product {product.Id}..");
            foreach (var image in changedImages)
            {
                await UpdateProduct_RemoveImage(product.Id, image.MediaId);

                // Same story as creating the images - do images that are only on the product separately from images that
                // are associated to a variant.
                var variantsWithThisImage = product.Variants.Where(v => v.MediaSrc == image.Src);
                if (!variantsWithThisImage.Any())
                {
                    _logger.LogInformation($"Adding image {image.Src} to product {product.Id}");
                    await UpdateProduct_AddImage(product.Id, image);
                }
                else
                {
                    _logger.LogInformation($"Adding image {image.Src} to {variantsWithThisImage.Count()} variants on product {product.Id}");
                    await UpdateProduct_AddNewImageToVariants(product.Id, variantsWithThisImage, image);
                }
            }

            // Remove any images that need to be deleted.
            if (product.MediaIdsToDelete.Any()) _logger.LogInformation($"Deleting {product.MediaIdsToDelete.Count()} images from product {product.Id}..");
            foreach (var imageId in product.MediaIdsToDelete)
            {
                await UpdateProduct_RemoveImage(product.Id, imageId);
            }
        }

        private async Task UpdateProduct_RemoveMetafields(ShopifyProductWithDeletes product)
        {
            if (product.MetafieldIdsToDelete.Any()) _logger.LogInformation($"Deleting {product.MetafieldIdsToDelete.Count()} metafields from product {product.Id}..");
            foreach (var metafieldId in product.MetafieldIdsToDelete)
            {
                await UpdateProduct_RemoveMetafield(metafieldId);
            }
        }

        private async Task UpdateProduct_RemoveMetafield(string metafieldId)
        {
            var query = @"
mutation metafieldDelete($input: MetafieldDeleteInput!) {
  metafieldDelete(input: $input) {
    deletedId
    userErrors {
      field
      message
    }
  }
}
";
            var parameters = new
            {
                input = new
                {
                    id = metafieldId
                }
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLMetafieldDeleteResult>(query, parameters, true);
            if (result.IsSuccessful()) return;

            var errors = result.ErrorList();
            if (errors.Any())
            {
                var firstError = errors.First();
                _logger.LogError(JsonConvert.SerializeObject(parameters, Formatting.Indented));
                throw new Exception($"Metafield deletion was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message}.  Note that due to Shopify bugs, Shopify may have updated the product anyways.");
            }
            else
            {
                throw new Exception("Metafield deletion was unsuccessful.  An unexpected or invalid response received from Shopify.");
            }
        }

        private async Task UpdateProduct_RemoveImage(string productId, string imageId)
        {
            var query = @"
mutation productDeleteMedia($id: ID!, $imageIds: [ID!]!) {
    productDeleteMedia(mediaIds: $imageIds, productId: $id) {
        mediaUserErrors {
            code
            field
            message
        }
    }
}
";
            var parameters = new
            {
                id = productId,
                imageIds = new string[] { imageId }
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductDeleteMediaResult>(query, parameters, true);
            if (result.IsSuccessful()) return;

            var errors = result.ErrorList();
            if (errors.Any())
            {
                var firstError = errors.First();
                _logger.LogError(JsonConvert.SerializeObject(parameters, Formatting.Indented));
                throw new Exception($"Product image deletion was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message}.  Note that due to Shopify bugs, Shopify may have updated the product anyways.");
            }
            else
            {
                throw new Exception("Product image deletion was unsuccessful.  An unexpected or invalid response received from Shopify.");
            }
        }

        private async Task<string> UpdateProduct_AddImage(string productId, ShopifyProductImage image)
        {
            var media = new ShopifyGraphQLProductCreateMediaInput()
            {
                mediaContentType = "IMAGE",
                originalSource = image.Src,
                alt = image.Alt,
            };

            var result = await CreateProduct_AddMediaBatch(productId, new ShopifyGraphQLProductCreateMediaInput[] { media });

            return result.First().id;
        }

        public async Task UpdateProduct_Data(ShopifyProduct product)
        {
            var input = new ShopifyGraphQLProductUpdateInputAdapter(product);
            var query = @"
mutation productUpdate($product: ProductUpdateInput) {
    productUpdate(product: $product) {
        product {
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
                product = input
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductUpdateResult>(query, parameters, true);
            if (result.IsSuccessful())
            {
                return;
            }

            var errors = result.ErrorList();
            if (errors.Any())
            {
                var firstError = errors.First();
                _logger.LogError(JsonConvert.SerializeObject(input, Formatting.Indented));
                throw new Exception($"Product update was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message}.  Note that due to Shopify bugs, Shopify may have updated the product anyways.");
            }
            else
            {
                throw new Exception("Product update was unsuccessful.  An unexpected or invalid response received from Shopify.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <returns>A Map of SKU to Variant id for any newly created variants.</returns>
        /// <exception cref="Exception"></exception>
        public async Task<IDictionary<string, string>> UpdateProduct_Variants(ShopifyProduct product)
        {
            var existingVariants = product.Variants.Where(v => !string.IsNullOrWhiteSpace(v.Id)).ToList();

            // Create any new variants.
            var variantIdMap = await CreateProduct_AddNewVariants(product);
            foreach (var variant in product.Variants)
            {
                if (string.IsNullOrWhiteSpace(variant.Id))
                {
                    if (variantIdMap.ContainsKey(variant.SKU))
                    {
                        variant.Id = variantIdMap[variant.SKU];
                    }
                    else
                    {
                        _logger.LogWarning($"For some reason, no variant was created for SKU {variant.SKU}.  This probably indicates a coding error.");
                    }
                }
            }

            // Update existing variants.
            var query = @"
mutation productVariantsBulkUpdate($productId: ID!, $variants: [ProductVariantsBulkInput!]!) {
    productVariantsBulkUpdate(productId: $productId, variants: $variants) {
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
            var parameters = new
            {
                productId = product.Id,
                variants = existingVariants.Select(v => new ShopifyGraphQLProductVariantsBulkInputAdapter(product, v))
            };

            var rval = new Dictionary<string, string>();
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductVariantBulkUpdateResult>(query, parameters, true);
            if (result.IsSuccessful())
            {
                foreach (var variant in result.productVariantsBulkUpdate.productVariants)
                {
                    rval.Add(variant.sku, variant.id);
                    product.Variants.Single(v => v.SKU == variant.sku).Id = variant.id;
                }
            }
            else
            {
                var errors = result.ErrorList();
                if (errors.Any())
                {
                    var firstError = errors.First();
                    throw new Exception($"Product variant creation for product {product.Id} was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(unknown field)" })}: {firstError.message}.  Note that due to Shopify bugs, Shopify may have created the variant anyways.  This will require manual intervention to correct.");
                }
                else
                {
                    throw new Exception($"Product variant creation for product {product.Id} was unsuccessful.  An unexpected or invalid response received from Shopify.  This will require manual intervention to correct.");
                }
            }

            // Remove any variant metafields that are no longer needed.
            var variantsWithMetafieldDeletions = product.Variants.Where(v => v is ShopifyProductVariantWithDeletes && (v as ShopifyProductVariantWithDeletes).MetafieldIdsToDelete.Any()).Cast<ShopifyProductVariantWithDeletes>();
            foreach( var variant in variantsWithMetafieldDeletions)
            {
                foreach (var metafieldId in variant.MetafieldIdsToDelete) {
                    _logger.LogInformation("Deleting metafield witth id {0} on variant {1}", metafieldId, variant.Id);
                    await UpdateProduct_RemoveMetafield(metafieldId);
                }
            }

            return rval;
        }

        public async Task UpdateProduct_AddNewImageToVariants(string productId, IEnumerable<ShopifyProductVariant> variants, ShopifyProductImage image)
        {
            var query = @"
mutation AddNewImageToVariants($productId: ID!, $variants: [ProductVariantsBulkInput!]!, $media: [CreateMediaInput!]) {
    productVariantsBulkUpdate(productId: $productId, media: $media, variants: $variants) {
        userErrors {
            field
            message
        }        
    }
}
";
            var variantsInput = variants.Select(v => new
            {
                id = v.Id,
                mediaSrc = v.MediaSrc
            });

            var mediaInput = new[] {
                new {
                    mediaContentType = "IMAGE",
                    originalSource = image.Src,
                    alt = image.Alt
                }
            };

            var parameters = new
            {
                media = mediaInput,
                productId = productId,
                variants = variantsInput,
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductVariantsBulkUpdateResult>(query, parameters, true);
            if (result.IsSuccessful())
            {
                return;
            }

            var errors = result.ErrorList();
            if (errors.Any())
            {
                var firstError = errors.First();
                throw new Exception($"Failed to add new image to variants for product ID {productId} and image {image.Src}.  First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message}.");
            }
            else
            {
                throw new Exception($"Failed to add new image to variants for product ID {productId} and image {image.Src}.  An unexpected or invalid response received from Shopify.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable IDE1006 // Naming Styles
namespace CQL.Middleware.Impl.Shopify.GraphQL.Models
{
    public class ShopifyGraphQLProductQueryResultData
    {
        public ShopifyGraphQLProduct? product { get; set; }
    }

    public class ShopifyGraphQLProductsQueryResultData
    {
        public ShopifyGraphQLResultPage<ShopifyGraphQLProduct>? products { get; set; }
    }

    public class ShopifyGraphQLProductChangeStatusResult
    {
        public ShopifyGraphQLProductUpdate? productChangeStatus { get; set; }

        public bool IsSuccessful()
        {
            if (productChangeStatus == null || productChangeStatus.product == null) return false;

            if (productChangeStatus.userErrors != null && productChangeStatus.userErrors.Any()) return false;

            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productChangeStatus?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLImageUpdateResult
    {
        public ShopifyGraphQLImageUpdate? productImageUpdate { get; set; }

        public bool IsSuccessful()
        {
            if (productImageUpdate == null || productImageUpdate.image == null) return false;

            if (productImageUpdate.userErrors != null && productImageUpdate.userErrors.Any()) return false;

            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productImageUpdate?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLImageUpdate
    {
        public ShopifyGraphQLMedia? image { get; set; }

        public IEnumerable<ShopifyGraphQLUserError> userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLProductUpdateResult
    {
        public ShopifyGraphQLProductUpdate? productUpdate { get; set; }

        public bool IsSuccessful()
        {
            if (productUpdate == null || productUpdate.product == null) return false;

            if (productUpdate.userErrors != null && productUpdate.userErrors.Any()) return false;

            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productUpdate?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLProductUpdate
    {
        public ShopifyGraphQLProduct? product { get; set; }

        public IEnumerable<ShopifyGraphQLUserError> userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLProductVariantsBulkUpdateResult
    {
        public ShopifyGraphQLProductVariantsBulkUpdate? productVariantsBulkUpdate { get; set; }
        public bool IsSuccessful()
        {
            if (productVariantsBulkUpdate == null) return false;

            if (productVariantsBulkUpdate.userErrors != null && productVariantsBulkUpdate.userErrors.Any()) return false;

            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productVariantsBulkUpdate?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLProductVariantsBulkUpdate
    {
        public ShopifyGraphQLProduct? product { get; set; }

        public List<ShopifyGraphQLProductVariant>? productVariants { get; set; }

        public IEnumerable<ShopifyGraphQLUserError>? userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();

    }

    public class ShopifyGraphQLProductCreateResult
    {
        public ShopifyGraphQLProductCreate? productCreate { get; set; }

        public bool IsSuccessful()
        {
            if (productCreate == null || productCreate.product == null) return false;

            if (productCreate.userErrors != null && productCreate.userErrors.Any()) return false;

            if (string.IsNullOrWhiteSpace(productCreate.product.Id)) return false;

            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productCreate?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLProductCreate
    {
        public ShopifyGraphQLProduct? product { get; set; }

        public IEnumerable<ShopifyGraphQLUserError> userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLProductDeleteResult
    {
        public ShopifyGraphQLProductDelete? productDelete { get; set; }

        public bool IsSuccessful()
        {
            if (productDelete == null) return false;

            if (productDelete.userErrors != null && productDelete.userErrors.Any()) return false;

            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productDelete?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLProductDelete
    {
        public string? deletedProductId { get; set; }

        public IEnumerable<ShopifyGraphQLUserError> userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLProduct
    {
        public string? Id { get; set; }

        public DateTime? createdAt { get; set; }

        public string? handle { get; set; }

        public string? productType { get; set; }

        public string? status { get; set; }

        public string? descriptionHtml { get; set; }

        public string? title { get; set; }

        public bool? tracksInventory { get; set; }

        public string? vendor { get; set; }

        public string[]? tags { get; set; }

        public ShopifyGraphQLProductOption[]? options { get; set; }


        public ShopifyGraphQLResultPage<ShopifyGraphQLProductVariant>? variants;

        public ShopifyGraphQLResultPage<ShopifyGraphQLMetafield>? metafields;

        //public ShopifyGraphQLResultPage<ShopifyGraphQLImage>? images;

        public ShopifyGraphQLResultPage<ShopifyGraphQLMedia>? media;
    }

    public class ShopifyGraphQLProductOption
    {
        public string? id { get; set; }

        public string? name { get; set; }

        public ShopifyGraphQLProductOptionValue[]? optionValues { get; set; }
    }

    public class ShopifyGraphQLProductOptionValue
    {
        public string id { get; set; }

        public string name { get; set; }    
    }

    public class ShopifyGraphQLProductVariantQueryResultData
    {
        public ShopifyGraphQLResultPage<ShopifyGraphQLProductVariant>? productVariants { get; set; }
    }

    public class ShopifyGraphQLProductVariantAppendMediaResult
    {
        public ShopifyGraphQLProductVariantAppendMediaResultData? productVariantAppendMedia { get; set; }

        public bool IsSuccessful()
        {
            if (productVariantAppendMedia == null) return false;
            if (productVariantAppendMedia.userErrors != null && productVariantAppendMedia.userErrors.Any()) return false;
            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productVariantAppendMedia?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLProductVariantAppendMediaResultData
    {
        public IEnumerable<ShopifyGraphQLUserError> userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLProductVariant
    {
        public string? sku { get; set; }

        public string? id { get; set; }

        public decimal? price { get; set; }

        public int? position { get; set; }

        public ShopifyGraphQLSelectedOption[]? selectedOptions { get; set; }

        public ShopifyGraphQLMetafield? metafield;

        public ShopifyGraphQLResultPage<ShopifyGraphQLMetafield>? metafields;

        public ShopifyGraphQLResultPage<ShopifyGraphQLMedia>? media;
    }

    public class ShopifyGraphQLSelectedOption
    {
        public string? name { get; set; }

        public string? value { get; set; }
    }

    public class ShopifyGraphQLProductMetafieldsQueryResultData
    {
        public ShopifyGraphQLProduct? product { get; set; }
    }

    public class ShopifyGraphQLProductVariantMetafieldsQueryResultData
    {
        public ShopifyGraphQLProductVariant? productVariant { get; set; }
    }

    public class ShopifyGraphQLProductImagesQueryResultData
    {
        public ShopifyGraphQLProduct? product { get; set; }
    }

    public class ShopifyGraphQLOptionCreateInput
    { 
        public string? id { get; set; }

        public string? name { get; set; }

        public int? position { get; set; }

        public ShopifyGraphQLProductOptionValueCreateInput[] values { get; set; }
    }

    public class ShopifyGraphQLProductOptionValueCreateInput
    {
        public string name { get; set; }
    }

    public class ShopifyGraphQLProductVariantUpdateResult
    {
        public ShopifyGraphQLProductVariantUpdateResultData? productVariantUpdate { get; set; }

        public bool IsSuccessful()
        {
            if (productVariantUpdate == null) return false;
            if (productVariantUpdate.userErrors != null && productVariantUpdate.userErrors.Any()) return false;
            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productVariantUpdate?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLProductVariantUpdateResultData
    {
        public ShopifyGraphQLUserError[]? userErrors { get; set; }
    }

    public class ShopifyGraphQLProductVariantBulkUpdateResult 
    {
        public ShopifyGraphQLProductVariantBulkUpdateResultData? productVariantsBulkUpdate { get; set; }

        public bool IsSuccessful()
        {
            if (productVariantsBulkUpdate == null) return false;
            if (productVariantsBulkUpdate.productVariants == null) return false;
            return productVariantsBulkUpdate.productVariants.All(v => !string.IsNullOrWhiteSpace(v.id) && !string.IsNullOrWhiteSpace(v.sku));
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productVariantsBulkUpdate?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLProductVariantBulkUpdateResultData : ShopifyGraphQLProductVariantBulkCreateResultData
    {
    }

    public class ShopifyGraphQLProductVariantBulkCreateResult
    {
        public ShopifyGraphQLProductVariantBulkCreateResultData? productVariantsBulkCreate { get; set; }

        public bool IsSuccessful()
        {
            if (productVariantsBulkCreate == null) return false;
            if (productVariantsBulkCreate.userErrors != null && productVariantsBulkCreate.userErrors.Any()) return false;
            if (productVariantsBulkCreate.productVariants == null) return false;
            return productVariantsBulkCreate.productVariants.All(v => !string.IsNullOrWhiteSpace(v.id) && !string.IsNullOrWhiteSpace(v.sku));
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productVariantsBulkCreate?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLProductVariantBulkCreateResultData
    {
        public ShopifyGraphQLProductVariant[]? productVariants { get; set; }

        public ShopifyGraphQLUserError[]? userErrors { get; set; }
    }

    public class ShopifyGraphQLProductVariantsBulkReorderResult
    {
        public ShopifyGraphQLProductVariantsBulkReorderResultData? productVariantsBulkReorder { get; set; }

        public bool IsSuccessful()
        {
            if (productVariantsBulkReorder?.userErrors?.Any() ?? false)
            {
                return false;
            }

            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productVariantsBulkReorder?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLProductVariantsBulkReorderResultData
    {
        public ShopifyGraphQLUserError[]? userErrors { get; set; }
    }

    public class ShopifyGraphQLProductOptionUpdateResult
    {
        public ShopifyGraphQLProductOptionUpdateResultData productOptionUpdate { get; set; }

        public bool IsSuccessful()
        {
            if (productOptionUpdate?.userErrors?.Any() ?? false)
            {
                return false;
            }

            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productOptionUpdate?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLProductOptionUpdateResultData
    {
        public ShopifyGraphQLUserError[]? userErrors { get; set; }
    }

    public class ShopifyGraphQLProductOptionsCreateResult
    {
        public ShopifyGraphQLProductOptionsCreateResultData productOptionsCreate { get; set; }

        public bool IsSuccessful()
        {
            if (productOptionsCreate?.userErrors?.Any() ?? false)
            {
                return false;
            }

            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productOptionsCreate?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLProductOptionsCreateResultData
    {
        public ShopifyGraphQLUserError[]? userErrors { get; set; }
    }
}

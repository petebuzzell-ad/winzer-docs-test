using Cql.Middleware.Library.Shopify.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Shopify.Products
{
    public interface IShopifyProductService
    {
        public Task<ShopifyProduct?> GetProduct(ShopifyProductQuery query, string productId);

        public Task<ShopifyProductResult> GetProducts(ShopifyProductQuery query, string? cursor = null);

        public Task<ShopifyCreateProductResult> CreateProduct(ShopifyProduct product);

        public Task<ShopifyUpdateProductResult> UpdateProduct(ShopifyProductWithDeletes product);

        public Task DeleteProduct(string shopifyProductId);

        public Task UpdateProductStatus(string productId, string status);
        public Task UpdateProductVariantPrices(string productId, IDictionary<string, decimal> variantPrices);
        public Task ReOrderProductVariants(string productId, IList<string> variantsInOrder);

        public Task<IEnumerable<ShopifyMetaField>> SetMetafields(IEnumerable<ShopifyMetafieldsSetInput> metafieldsInput, CancellationToken cancellationToken = default);
    }

    public class ShopifyCreateProductResult
    {
        public string ProductId { get; set; } = string.Empty;
        public IDictionary<string, string> VariantSKUToIdMap { get; set; } = new Dictionary<string, string>();
        public string? Handle { get; set; } = string.Empty;
    }

    public class ShopifyUpdateProductResult
    {
        public IDictionary<string, string> NewVariantsSKUToIdMap { get; set; } = new Dictionary<string, string>();
    }

    public class ShopifyBulkUpdateVariantsResult
    {
        public string ProductId { get; set; } = string.Empty;

        public List<ShopifyProductVariant>? productVariants { get; set; }
    }

    public class ShopifyDeleteProductResult
    {
        public string ProductId { get; set; } = string.Empty;
    }

    public class ShopifyProductResult
    {
        public bool HasMoreResults { get; set; }

        public string? Cursor { get; set; }

        public IList<ShopifyProduct>? Products { get; set; }
    }

    public class ShopifyProductQuery
    {
        public string[]? ProductFields { get; set; }

        public bool IncludeVariantsInResults { get; set; }

        public string[]? VariantFields { get; set; }

        public bool IncludeImagesInResults { get; set; }

        public string[]? ImageFields { get; set; }

        public bool IncludeMetafieldsInResults { get; set; }

        public bool IncludeVariantMetafieldsInResults { get; set; }

        public string? MetafieldNamespace { get; set; }
    }
}

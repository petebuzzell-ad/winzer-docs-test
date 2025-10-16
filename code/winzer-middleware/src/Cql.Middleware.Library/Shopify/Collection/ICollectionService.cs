using Cql.Middleware.Library.Shopify.Products;

namespace Cql.Middleware.Library.Shopify.Collection
{
    public interface ICollectionService
    {
        public Task<ShopifyCollectionsQueryResult?> GetAllCollections(string cursor = "");
        public Task<ShopifyCollectionResult?> GetCollectionByHandle(string collectionHandle);
        public Task<ShopifyCollectionResult> CreateCollection(CollectionItem collection);
        public Task<string?> GetCategoriesMetafield();
    }

    public class ShopifyCollectionsQueryResult
    {
        public bool HasMoreResults { get; set; }

        public string? Cursor { get; set; }

        public IList<CollectionItem>? Collections { get; set; }
    }

    public class ShopifyCollectionResult
    {
        public string Id { get; set; }
    }
}

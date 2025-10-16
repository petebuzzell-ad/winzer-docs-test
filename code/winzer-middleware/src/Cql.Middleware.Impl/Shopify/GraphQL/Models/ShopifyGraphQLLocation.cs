namespace CQL.Middleware.Impl.Shopify.GraphQL.Models
{
    public class ShopifyGraphQLLocation
    {
        public String? id { get; set; }
        public String? name { get; set; }
        public Boolean? activatable { get; set; }
        public Boolean? deactivatable { get; set; }
        public String? deactivatedAt { get; set; }
        public Boolean? deletable { get; set; }
        public Boolean? fulfillsOnlineOrders { get; set; }
        public Boolean? hasActiveInventory { get; set; }
        public Boolean? hasUnfulfilledOrders { get; set; }
        public Boolean? isActive { get; set; }
        public String? legacyResourceId { get; set; }
        public Boolean? shipsInventory { get; set; }
        public ShopifyGraphQLResultPage<ShopifyGraphQLInventoryLevel>? inventoryLevels { get; set; }
    }
}

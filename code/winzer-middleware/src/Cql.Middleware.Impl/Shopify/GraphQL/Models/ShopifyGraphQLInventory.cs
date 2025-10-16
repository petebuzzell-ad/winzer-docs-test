#pragma warning disable IDE1006 // Naming Styles
namespace CQL.Middleware.Impl.Shopify.GraphQL.Models
{
    public class ShopifyGraphQLInventoryLookupResultData
    {
        public ShopifyGraphQLResultPage<ShopifyGraphQLInventoryItem>? inventoryItems { get; set; }
    }

    public class ShopifyGraphQLInventoryBulkLookupResultData
    {
        public ShopifyGraphQLLocation? location { get; set; }
    }

    public class ShopifyGraphQLInventoryBulkUpdateResultData
    {
        public ShopifyGraphQLInventoryBulkAdjustQuantityAtLocation inventoryBulkAdjustQuantityAtLocation { get; set; } = new ShopifyGraphQLInventoryBulkAdjustQuantityAtLocation();
    }

    public class ShopifyGraphQLInventoryActivateResultData
    {
        public ShopifyGraphQLInventoryActivate inventoryActivate { get; set; } = new ShopifyGraphQLInventoryActivate();
    }

    public class ShopifyGraphQLInventoryBulkAdjustQuantityAtLocation
    {
        public IEnumerable<ShopifyGraphQLInventoryLevel> inventoryLevels { get; set; } = Enumerable.Empty<ShopifyGraphQLInventoryLevel>();
        public IEnumerable<ShopifyGraphQLUserError> userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLInventoryActivate
    {
        public ShopifyGraphQLInventoryLevel? inventoryLevel { get; set; }
        public IEnumerable<ShopifyGraphQLUserError> userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLInventoryItem
    {
        public String? id { get; set; }
        public String? sku { get; set; }
        public String? countryCodeOfOrigin { get; set; }
        public Int32? duplicateSkuCount { get; set; }
        public String? harmonizedSystemCode { get; set; }
        public Uri? inventoryHistoryUrl { get; set; }
        public String? legacyResourceId { get; set; }
        public Int32? locationsCount { get; set; }
        public String? provinceCodeOfOrigin { get; set; }
        public Boolean? requiresShipping { get; set; }
        public Boolean? tracked { get; set; }
        public DateTimeOffset? createdAt { get; set; }
        public DateTimeOffset? updatedAt { get; set; }
        public ShopifyGraphQLEditableProperty? trackedEditable { get; set; }
        public ShopifyGraphQLMoney? unitCost { get; set; }
        public ShopifyGraphQLInventoryLevel? inventoryLevel { get; set; }
        public ShopifyGraphQLResultPage<ShopifyGraphQLInventoryLevel>? inventoryLevels { get; set; }
        public ShopifyGraphQLProductVariant? variant { get; set; }
    }

    public class ShopifyGraphQLInventoryLevel
    {
        public String? id { get; set; }
        public Int32? available { get; set; }
        public Boolean? canDeactivate { get; set; }
        public String? deactivationAlert { get; set; }
        public String? deactivationAlertHtml { get; set; }
        public Int32? incoming { get; set; }
        public DateTimeOffset? createdAt { get; set; }
        public DateTimeOffset? updatedAt { get; set; }
        public ShopifyGraphQLInventoryItem? item { get; set; }
        public ShopifyGraphQLLocation? location { get; set; }
    }
}

using Cql.Middleware.Library.Shopify.Common;

namespace Cql.Middleware.Library.Shopify.Order
{
    public interface IOrderService: IBaseService
    {
        public Task<ShopifyOrder> LookupOrder(ShopifyOrderQuery query, CancellationToken cancellationToken = default);
        public Task<IEnumerable<ShopifyOrder>> LookupOrders(ShopifyOrderQuery query, CancellationToken cancellationToken = default);
        public Task<ShopifyOrder> UpdateOrderTags(String Id, IEnumerable<String> tags, CancellationToken cancellationToken = default);
        public Task<IEnumerable<ShopifyOrder>> LookupFailedOrders(ShopifyOrderQuery query, CancellationToken cancellationToken = default);
    }
    public interface IBaseService
    {
        public Task<IEnumerable<ShopifyMetaField>> SetMetafields(IEnumerable<ShopifyMetafieldsSetInput> metafieldsInput, CancellationToken cancellationToken = default);
    }

    public class OrderOptions
    {
        public int NumOrders { get; set; } = 10;
        public int NumLineItems { get; set; } = 2;
        public int NumFailedOrders { get; set; } = 30;
        public int NumAdditionalLineItems { get; set; } = 30;
        public int FailedOrdersToHoursAgo { get; set; } = 4;
        public int FailedOrdersFromHoursAgo { get; set; } = 28;
        public String OrderFolder { get; set; } = "orders";
        public String ExportedTag { get; set; } = "Exported";
        public String NotExportedTag { get; set; } = "NotExported-BadData";
        public String FraudOverideTag { get; set; } = "FraudOveride";
        public String LegacyOrderImportTag { get; set; } = "LegacyOrderImported";
    }

    public class ShopifyOrderQuery : OrderOptions
    {
        public ShopifyOrderQuery() { }

        public ShopifyOrderQuery(OrderOptions options)
        {
            NumOrders = options.NumOrders;
            NumLineItems = options.NumLineItems;
            NumFailedOrders = options.NumFailedOrders;
            NumAdditionalLineItems = options.NumAdditionalLineItems;
            OrderFolder = options.OrderFolder;
        }
        public string? OrderId { get; set; } = String.Empty;
        public string? OrdersQuery { get; set; } = String.Empty;
        public bool IncludeTransactions { get; set; }
        public bool IncludeShippingLine { get; set; }
        public bool IncludeShippingAddress { get; set; }
        public bool IncludeBillingAddress { get; set; }
        public bool IncludeLineItems { get; set; }
        public bool IncludeLineItemVariant { get; set; }
        public bool IncludeLineItemProduct { get; set; }
        public String LineItemVariantMetafieldsKey { get; set; } = String.Empty;
        public String MetafieldNamespace { get; set; } = String.Empty;
        public bool IncludeFulfillmentOrders { get; set; }
        public bool IncludeFulfillments { get; set; }
        public bool IncludeMetafields { get; set; }
    }
}

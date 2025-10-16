namespace CQL.Middleware.Impl.Shopify.GraphQL.Models;

public class ShopifyGraphQLFulfillmentCreateResultData
{
    public ShopifyGraphQLFulfillmentCreate fulfillmentCreateV2 { get; set; } = new ShopifyGraphQLFulfillmentCreate();
}

public class ShopifyGraphQLFulfillmentCreate
{
    public ShopifyGraphQLFulfillment? fulfillment { get; set; }
    public IEnumerable<ShopifyGraphQLUserError> userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
}

public class ShopifyGraphQLFulfillmentResultData
{
    public ShopifyGraphQLFulfillment fulfillment { get; set; } = new ShopifyGraphQLFulfillment();
}

public class ShopifyGraphQLFulfillment
{
    public string? id { get; set; }
    public IEnumerable<ShopifyGraphQLFulfillmentTrackingInfo> trackingInfo { get; set; } = Enumerable.Empty<ShopifyGraphQLFulfillmentTrackingInfo>();
    public ShopifyGraphQLResultPage<ShopifyGraphQLFulfillmentLineItem>? fulfillmentLineItems { get; set; }
    public DateTime? createdAt { get; set; }
    public DateTime? updatedAt { get; set; }
}

public class ShopifyGraphQLFulfillmentLineItem
{
    public string? id { get; set; }
    public int? quantity { get; set; }
    public ShopifyGraphQLLineItem? lineItem { get; set; }
}

public class ShopifyGraphQLFulfillmentTrackingInfo
{
    public string? number { get; set; }
    public string? url { get; set; }
    public string? company { get; set; }
}

public class ShopifyGraphQLFulfillmentOrder
{
    public string? id { get; set; }
    public ShopifyGraphQLResultPage<ShopifyGraphQLFulfillmentOrderLineItem>? lineItems { get; set; }
    public string? status { get; set; }
    public ShopifyGraphQLDeliveryMethod? deliveryMethod { get; set; }
}

public class ShopifyGraphQLDeliveryMethod
{
    public string? id { get; set; }
    public DateTime? maxDeliveryDateTime { get; set; }
    public string? methodType { get; set; }
    public DateTime? minDeliveryDateTime { get; set; }
}

public class ShopifyGraphQLFulfillmentOrderLineItem
{
    public string? id { get; set; }
    public ShopifyGraphQLLineItem? lineItem { get; set; }
    public int? remainingQuantity { get; set; }
    public int? totalQuantity { get; set; }
}

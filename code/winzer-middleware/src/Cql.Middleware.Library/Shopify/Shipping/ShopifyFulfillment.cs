using Cql.Middleware.Library.Shopify.Order;
namespace Cql.Middleware.Library.Shopify.Shipping;

/// <summary>
/// Represents a fulfillment. In Shopify, a fulfillment represents a shipment of one or more items in an order. When an order has been completely fulfilled, it means that all the items that are included in the order have been sent to the customer. There can be more than one fulfillment for an order.
/// </summary>
public class ShopifyFulfillment
{
    /// <summary>
    /// A globally-unique identifier
    /// </summary
    public string Id { get; set; } = String.Empty;
    /// <summary>
    /// Tracking information associated with the fulfillment,
    /// such as the tracking company, tracking number, and tracking URL.
    /// </summary
    public IEnumerable<ShopifyFulfillmentTrackingInfo> TrackingInfo { get; set; } = Enumerable.Empty<ShopifyFulfillmentTrackingInfo>();

    /// <summary>
    /// List of the fulfillment's line items.
    /// </summary
    public IEnumerable<ShopifyFulfillmentLineItem> FulfillmentLineItems { get; set; } = Enumerable.Empty<ShopifyFulfillmentLineItem>();
    public ShopifyOrder? Order { get; set; }

    /// <summary>
    /// The date and time when the fulfillment was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the fulfillment was last modified.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

public class ShopifyFulfillmentLineItem
{
    public string Id { get; set; } = String.Empty;
    public int? Quantity { get; set; }
    public ShopifyLineItem LineItem { get; set; } = new ShopifyLineItem();
}

public class ShopifyFulfillmentTrackingInfo
{
    /// <summary>
    /// The tracking number of the fulfillment.
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// The URL to track the fulfillment.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// The name of the tracking company.
    /// </summary>
    public string? Company { get; set; }
}


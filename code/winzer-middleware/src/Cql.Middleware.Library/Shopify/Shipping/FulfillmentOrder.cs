using Cql.Middleware.Library.Shopify.Order;

namespace Cql.Middleware.Library.Shopify.Shipping;

/// <summary>
/// Represents a fulfillment order. In Shopify, a fulfillment order represents a group of one or more items
/// in an order that are to be fulfilled from the same location. There can be more than one fulfillment order
/// for an order at a given location..
/// </summary>
public class FulfillmentOrder
{
    /// <summary>
    /// A globally-unique identifier
    /// </summary
    public string Id { get; set; } = String.Empty;
    /// <summary>
    /// Delivery method of this fulfillment order.
    /// </summary
   public DeliveryMethod DeliveryMethod { get; set; } = new DeliveryMethod();
    /// <summary>
    ///  A list of the fulfillment order's line items.
    /// </summary
    public IEnumerable<FulfillmentOrderLineItem> LineItems { get; set; } = Enumerable.Empty<FulfillmentOrderLineItem>();
    /// <summary>
    ///  The status of the fulfillment order.
    /// </summary
    public string Status { get; set; } = String.Empty;
}

/// <summary>
/// The delivery method used by a fulfillment order.
/// </summary>
public class DeliveryMethod
{
    /// <summary>
    ///  A globally-unique identifier.
    /// </summary
    public string Id { get; set; } = String.Empty;
    /// <summary>
    ///  The maximum date and time by which the delivery is expected to be completed.
    /// </summary
    public DateTime MaxDeliveryDateTime { get; set; }
    /// <summary>
    ///  The type of the delivery method.
    /// </summary
    public string MethodType { get; set; } = String.Empty;
    /// <summary>
    ///  The minimum date and time by which the delivery is expected to be completed.
    /// </summary
    public DateTime MinDeliveryDateTime { get; set; }
}

/// <summary>
/// Represents a line item belonging to a fulfillment order.
/// </summary
public class FulfillmentOrderLineItem
{
    /// <summary>
    /// A globally-unique identifier
    /// </summary
    public string Id { get; set; } = String.Empty;
    /// <summary>
    /// The associated order line item.
    /// </summary
    public ShopifyLineItem LineItem { get; set; } = new ShopifyLineItem();
    /// <summary>
    /// The number of units remaining to be fulfilled.
    /// </summary
    public int RemainingQuantity { get; set; }
    /// <summary>
    /// The total number of units to be fulfilled.
    /// </summary
    public int totalQuantity { get; set; }
}

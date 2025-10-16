namespace Cql.Middleware.Library.Shopify.Shipping;

/// <summary>
/// The input fields used to create a fulfillment from fulfillment orders.
/// </summary>
public class FulfillmmentInput
{
    /// <summary>
    /// The ID of the order.  Used Internally.
    /// </summary>
    public string OrderId { get; set; } = String.Empty;
    /// <summary>
    /// The fulfillment's tracking information, including a tracking URL, a tracking number,
    /// and the company associated with the fulfillment.
    /// </summary>
    public FulfillmentTrackingInput? TrackingInfo { get; set; }

    /// <summary>
    /// Whether the customer is notified.
    /// If `true`, then a notification is sent when the fulfillment is created.
    /// </summary>
    public bool? NotifyCustomer { get; set; }

    /// <summary>
    /// Pairs of `fulfillment_order_id` and `fulfillment_order_line_items` that represent the fulfillment
    /// order line items that have to be fulfilled for each fulfillment order.  For any given pair, if the
    /// fulfillment order line items are left blank then all the fulfillment order line items of the
    /// associated fulfillment order ID will be fulfilled.
    /// </summary>
    public IEnumerable<FulfillmentOrderLineItemsInput> LineItemsByFulfillmentOrder { get; set; } = Enumerable.Empty<FulfillmentOrderLineItemsInput>();
}

/// <summary>
/// The input fields that specify all possible fields for tracking information.
/// </summary>
public class FulfillmentTrackingInput
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

/// <summary>
/// The input fields used to include the line items of a specified fulfillment order that should be fulfilled.
/// </summary>
public class FulfillmentOrderLineItemsInput
{
    /// <summary>
    /// The ID of the fulfillment order.
    /// </summary>
    public string FulfillmentOrderId { get; set; } = String.Empty;

    /// <summary>
    /// The fulfillment order line items to be fulfilled.
    /// If left blank, all line items of the fulfillment order will be fulfilled.
    /// </summary>
    public IEnumerable<FulfillmentOrderLineItemInput> FulfillmentOrderLineItems { get; set; } = Enumerable.Empty<FulfillmentOrderLineItemInput>();
}

/// <summary>
/// The input fields used to include the quantity of the fulfillment order line item that should be fulfilled.
/// </summary>
public class FulfillmentOrderLineItemInput
{
    /// <summary>
    /// The input fields used to include the quantity of the fulfillment order line item that should be fulfilled.
    /// </summary>
    public string Id { get; set; } = String.Empty;

    /// <summary>
    /// The quantity of the fulfillment order line item.
    /// </summary>
    public int? Quantity { get; set; }
}

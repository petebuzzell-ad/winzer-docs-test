using Cql.Middleware.Library.Shopify.Store;

namespace Cql.Middleware.Library.Shopify.Shipping;

public class ShopifyFulfillmentService
{
    /// <summary>
    /// The callback URL the fulfillment service has registered for requests.
    /// </summary>
    public String? CallbackUrl { get; set; }
    /// <summary>
    /// Whether the fulfillment service has opted into fulfillment order based requests.
    /// </summary>
    public Boolean FulfillmentOrdersOptIn { get; set; }
    /// <summary>
    /// Human-readable unique identifier for this fulfillment service.
    /// </summary>
    public String Handle { get; set; } = String.Empty;
    /// <summary>
    /// The ID of the fulfillment service.
    /// </summary>
    public String Id { get; set; } = String.Empty;
    /// <summary>
    /// Whether the fulfillment service tracks product inventory and provides updates to Shopify.
    /// </summary>
    public Boolean InventoryManagement { get; set; }
    /// <summary>
    /// Location associated with the fulfillment service.
    /// </summary>
    public Location? Location { get; set; }
    /// <summary>
    /// Whether the fulfillment service supports local deliveries.
    /// </summary>
    public Boolean ProductBased { get; set; }
    /// <summary>
    /// The name of the fulfillment service as seen by merchants.
    /// </summary>
    public String ServiceName { get; set; } = String.Empty;
    /// <summary>
    /// Shipping methods associated with the fulfillment service provider.
    /// </summary>
    public IEnumerable<ShopifyShippingMethod> ShippingMethods { get; set; } = Enumerable.Empty<ShopifyShippingMethod>();
    /// <summary>
    /// Type associated with the fulfillment service.
    /// </summary>
    public String Type { get; set; } = String.Empty;
}

namespace Cql.Middleware.Library.Shopify.Shipping;

public class ShopifyShippingMethod
{
    /// <summary>
    /// A unique code associated with the rate. For example: `expedited_mail`
    /// </summary>
    public String Code { get; set; } = String.Empty;

    /// <summary>
    /// A description of the rate, which customers will see at checkout.
    /// For example: `Local delivery`, `Free Express Worldwide`, `Includes tracking and insurance`.
    /// </summary>
    public String Label { get; set; } = String.Empty;
}

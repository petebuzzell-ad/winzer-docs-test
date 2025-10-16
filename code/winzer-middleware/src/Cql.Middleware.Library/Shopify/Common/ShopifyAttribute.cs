namespace Cql.Middleware.Library.Shopify.Common;

public class ShopifyAttribute {
    /// <summary>
    /// Key or name of the attribute.
    /// </summary>
    public String Key { get; set; } = String.Empty;

    /// <summary>
    /// Value of the attribute.
    /// </summary>
    public String? Value { get; set; }
}

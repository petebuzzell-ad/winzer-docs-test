namespace Cql.Middleware.Library.Shopify.Common;

public class ShopifyPrivateMetafield
{
    /// <summary>
    /// The date and time when the private metafield was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// The id of the private metafield.
    /// </summary>
    public String Id { get; set; } = String.Empty;
    /// <summary>
    /// The key name of the private metafield.
    /// </summary>
    public String Key { get; set; } = String.Empty;
    /// <summary>
    /// The namespace of the private metafield.
    /// </summary>
    public String Namespace { get; set; } = String.Empty;
    /// <summary>
    /// The date and time when the private metafield was updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    /// <summary>
    /// The value of a private metafield.
    /// </summary>
    public String Value { get; set; } = String.Empty;
    /// <summary>
    /// Represents the private metafield value type.
    /// </summary>
    public String ValueType { get; set; } = String.Empty;
}

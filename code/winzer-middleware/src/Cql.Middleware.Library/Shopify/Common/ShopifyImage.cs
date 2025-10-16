namespace Cql.Middleware.Library.Shopify.Common;

public class ShopifyImage
{
    public static readonly ShopifyImage Empty = new ShopifyImage();
    /// <summary>
    /// A word or phrase to share the nature or contents of an image.
    /// </summary>
    public String? AltText { get; set; }
    /// <summary>
    /// The original height of the image in pixels. Returns `null` if the image is not hosted by Shopify.
    /// </summary>
    public int Height { get; set; }
    /// <summary>
    /// A unique identifier for the image.
    /// </summary>
    public String Id { get; set; } = String.Empty;
    /// <summary>
    /// Returns a metafield by namespace and key that belongs to the resource.
    /// </summary>
    public ShopifyMetaField? Metafield { get; set; }
    /// <summary>
    /// List of metafields that belong to the resource.
    /// </summary>
    public IEnumerable<ShopifyMetaField> Metafields { get; set; } = Enumerable.Empty<ShopifyMetaField>();
    /// <summary>
    /// Returns a private metafield by namespace and key that belongs to the resource.
    /// </summary>
    public ShopifyPrivateMetafield? PrivateMetafield { get; set; }
    /// <summary>
    /// List of private metafields that belong to the resource.
    /// </summary>
    public IEnumerable<ShopifyPrivateMetafield> PrivateMetafields { get; set; } = Enumerable.Empty<ShopifyPrivateMetafield>();
    /// <summary>
    /// The location of the image as a URL.
    /// If no transform options are specified, then the original image will be preserved including any pre-applied transforms.
    /// All transformation options are considered \"best-effort\". Any transformation that the original image type doesn't support will be ignored.
    /// If you need multiple variations of the same image, then you can use [GraphQL aliases](https:\/\/graphql.org\/learn\/queries\/#aliases).
    /// </summary>
    public String Url { get; set; } = String.Empty;
    /// <summary>
    /// The original width of the image in pixels. Returns `null` if the image is not hosted by Shopify.
    /// </summary>
    public int Width { get; set; }
}

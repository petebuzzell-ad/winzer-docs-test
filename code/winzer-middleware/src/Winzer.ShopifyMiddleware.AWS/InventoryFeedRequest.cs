namespace Winzer.ShopifyMiddleware.AWS;

public class InventoryFeedRequest
{
    public IEnumerable<String> FileNames { get; set; } = new List<String>();
    public String? Path { get; set; }    // /sftp/inventory
    public String? Pattern { get; set; } // BRIGHTON-INV-.*.txt
    public bool Dryrun { get; set; } = true;
    public String? LocationId { get; set; } = String.Empty; // gid://shopify/Location/65023344820

}

public class InventoryFeedResult
{
    public string FileName { get; set; } = String.Empty;
    public string Message { get; set; } = String.Empty;
}

public class InventoryFeedResponse
{
    public IEnumerable<InventoryFeedResult> Results { get; set; } = Enumerable.Empty<InventoryFeedResult>();
}

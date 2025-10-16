namespace Cql.Middleware.Library.Shopify.Inventory
{
    public interface IInventoryService
    {
        public Task<IEnumerable<InventoryItem>> LookupInventory(ShopifyInventoryQuery query, CancellationToken cancellationToken = default);
        public IAsyncEnumerable<InventoryLevel> InventoryBulkLookup(ShopifyInventoryQuery query, CancellationToken cancellationToken = default);
        public Task<int> InventoryBulkUpdate(IEnumerable<InventoryAdjustQuantity> items, CancellationToken cancellationToken = default);
        public Task<int> InventoryActivate(IEnumerable<InventoryAdjustQuantity> items, CancellationToken cancellationToken = default);
    }

    public class InventoryOptions
    {
        public int NumBulkInventoryItems { get; set; } = 250;
        public int NumInventoryItems { get; set; } = 10;
        public int BulkUpdateChunkSize { get; set; } = 50;
        public String StoreIdLookupFileName { get; set; } = String.Empty;
        public bool WarnDuplicateSkus { get; set; } = true;
        public bool ArchiveInventoryFile { get; set; } = true;
        public String ArchiveFolder { get; set; } = "archive";
        public bool ActivateInventorySkus { get; set; } = true;
        public bool CopyInventoryFileForSalsify { get; set; } = true;
        public String SalsifyInventoryFileName { get; set; } = "last_processed_inventory.csv";
    }

    public class ShopifyInventoryQuery : InventoryOptions
    {
        public ShopifyInventoryQuery() {}

        public ShopifyInventoryQuery(InventoryOptions options)
        {
            NumBulkInventoryItems = options.NumBulkInventoryItems;
            NumInventoryItems = options.NumInventoryItems;
            BulkUpdateChunkSize = options.BulkUpdateChunkSize;
            StoreIdLookupFileName = options.StoreIdLookupFileName;
            WarnDuplicateSkus = options.WarnDuplicateSkus;
            ArchiveFolder = options.ArchiveFolder;
            ActivateInventorySkus = options.ActivateInventorySkus;
        }

        public string[]? InventoryItemFields { get; set; } = new string[] { "id", "sku", "duplicateSkuCount" };
        public string[]? InventoryLevelFields { get; set; } = new string[] { "id", "available" };
        public string? LocationId { get; set; }
        public string? Sku { get; set; }
    }
}

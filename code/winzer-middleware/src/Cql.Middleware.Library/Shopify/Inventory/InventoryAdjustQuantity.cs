namespace Cql.Middleware.Library.Shopify.Inventory;

public class InventoryAdjustQuantity {
    public String InventoryItemId { get; set; } = String.Empty;
    public int AvailableDelta { get; set; }
    public String LocationId { get; set; } = String.Empty;

}

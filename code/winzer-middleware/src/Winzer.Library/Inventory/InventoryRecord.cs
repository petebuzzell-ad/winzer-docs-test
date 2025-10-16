namespace Winzer.Library.Inventory;

public class InventoryRecord
{
    public string Sku { get; set; } = String.Empty;
    public int Inventory { get; set; } = 0;
    public string StoreId { get; set; } = String.Empty;
    public string ProductInventoryLifecycle { get; set; } = String.Empty;
    public string IsNew { get; set; } = String.Empty;
}

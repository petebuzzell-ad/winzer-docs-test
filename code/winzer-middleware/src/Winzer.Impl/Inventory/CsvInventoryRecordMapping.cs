using Winzer.Library.Inventory;
using CsvHelper.Configuration;

namespace Winzer.Impl.Inventory;

public class CsvInventoryRecordMapping : ClassMap<InventoryRecord>
{
    public CsvInventoryRecordMapping()
    {
        Map(m => m.Sku).Index(0);
        Map(m => m.Inventory).Index(3);
        Map(m => m.ProductInventoryLifecycle).Index(6);
        Map(m => m.IsNew).Index(7);
    }
}

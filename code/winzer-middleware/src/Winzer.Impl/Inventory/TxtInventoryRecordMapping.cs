using Winzer.Library.Inventory;
using CsvHelper.Configuration;

namespace Winzer.Impl.Inventory;

public class TxtInventoryRecordMapping : ClassMap<InventoryRecord>
{
    public TxtInventoryRecordMapping()
    {
        Map(m => m.Sku).Index(0);
        Map(m => m.Inventory).Index(1);
        Map(m => m.StoreId).Index(2);
    }
}

using Winzer.Library.Store;
using CsvHelper.Configuration;

namespace Winzer.Impl.Store;

public class CsvStoreLocationRecordMapping : ClassMap<StoreLocationRecord>
{
    public CsvStoreLocationRecordMapping()
    {
        Map(m => m.StoreId).Index(0);
        Map(m => m.LocationId).Index(1);
    }
}

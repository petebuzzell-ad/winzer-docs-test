using Winzer.Library.Fulfillment;
using CsvHelper.Configuration;

namespace Winzer.Impl.Fulfillment;

public class CsvFulfillmentRecordMapping : ClassMap<FulfillmentRecord>
{
    public CsvFulfillmentRecordMapping()
    {
        Map(m => m.TrackingNumber).Index(0);
        Map(m => m.OrderId).Index(1);
        Map(m => m.Carrier).Index(2);
        Map(m => m.StyleNumbers).Index(3);
    }
}

using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace Winzer.Impl.Fulfillment;

public class KWINoteRecord
{
    [Index(0)]
    public String? ObjectName { get; set; }
    [Index(1)]
    [StringLength(9)]
    public String? OrderNum { get; set; }
    [Index(2)]
    [StringLength(9)]
    public String? ItemRefNum { get; set; }
    [Index(3)]
    [StringLength(8)]
    public String? ShipDate { get; set; }
    [Index(4)]
    public String? ItemShipMthd { get; set; }
    [Index(5)]
    public String? PkgTrackNumUM { get; set; }
    [Index(6)]
    public String? Quantity { get; set; }
    [Index(7)]
    [StringLength(8)]
    public String? InvoiceDate { get; set; }
    [Index(8)]
    public String? VendorRef { get; set; }
    [Index(9)]
    public String? ItemExpRcpt { get; set; }
    [Index(10)]
    public String? DiscDate { get; set; }
    [Index(11)]
    public String? ReasonCode { get; set; }
    [Index(12)]
    public String? ReturnCode { get; set; }
    [Index(13)]
    public String? VendorPrice { get; set; }
    [Index(14)]
    [Constant("340400")]
    public String? DecrementingLocation { get; set; }
    [Index(15)]
    public String? ShippingReturnAmount { get; set; }
    [Index(16)]
    public String? ShippingReturnTax { get; set; }
    [Index(17)]
    public String? ActualShippingCharges { get; set; }
    [Index(18)]
    public String? ActualShippingWeight { get; set; }
    [Index(19)]
    [StringLength(8)]
    public String? ShipDate1 { get; set; }
    [Index(20)]
    public String? ItemShipMethod { get; set; }
    [Index(21)]
    public String? ReturnDate { get; set; }
}

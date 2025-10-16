using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

namespace Winzer.Impl.Order;

public class KWIOrderRecord
{
    [Index(0)]
    [StringLength(9)]
    public String? OrderNumber { get; set; }
    [Index(1)]
    [StringLength(10)]
    public String? OrderSource { get; set; }
    [Index(2)]
    [StringLength(10)]
    public String? OrderDate { get; set; }
    [Index(3)]
    [StringLength(5)]
    public String? OrderTime { get; set; }
    [Index(4)]
    [StringLength(9)]
    public String? OrderSalesTax { get; set; }
    [Index(5)]
    [StringLength(6)]
    [Constant("340400")]
    public String? OrderStoreNum { get; set; }
    [Index(6)]
    [StringLength(9)]
    public String? OrderShipCharge { get; set; }
    [Index(7)]
    [StringLength(9)]
    public String? OrderMerchandiseTotal { get; set; }

    [Index(8)]
    [StringLength(20)]
    public String? BillToFirstName { get; set; }
    [Index(9)]
    [StringLength(30)]
    public String? BillToLastName { get; set; }
    [Index(10)]
    [StringLength(30)]
    public String? BillToAddress1 { get; set; }
    [Index(11)]
    [StringLength(30)]
    public String? BillToAddress2 { get; set; }
    [Index(12)]
    [StringLength(30)]
    public String? BillToCity { get; set; }
    [Index(13)]
    [StringLength(2)]
    public String? BillToState { get; set; }
    [Index(14)]
    [StringLength(10)]
    public String? BillToZip { get; set; }
    [Index(15)]
    [StringLength(10)]
    public String? BillToCountry { get; set; }
    [Index(16)]
    [StringLength(14)]
    public String? BillToTelephone { get; set; }
    [Index(17)]
    [StringLength(60)]
    public String? BillToEmail { get; set; }

    [Index(18)]
    public String? DoNotPromote { get; set; }

    [Index(19)]
    [StringLength(2)]
    public String? PaymentType1 { get; set; }
    [Index(20)]
    [StringLength(4)]
    public String? PaymentType1Num { get; set; }
    [Index(21)]
    [StringLength(4)]
    public String? PaymentType1Expires { get; set; }
    [Index(22)]
    [StringLength(15)]
    public String? PaymentType1AuthCode { get; set; }
    [Index(23)]
    [StringLength(8)]
    public String? PaymentType1AuthDate { get; set; }
    [Index(24)]
    [StringLength(9)]
    public String? PaymentType1Total { get; set; }

    [Index(25)]
    [StringLength(10)]
    public String? PaymentType2 { get; set; }
    [Index(26)]
    [StringLength(4)]
    public String? PaymentType2Num { get; set; }
    [Index(27)]
    [StringLength(4)]
    public String? PaymentType2Expires { get; set; }
    [Index(28)]
    [StringLength(15)]
    public String? PaymentType2AuthCode { get; set; }
    [Index(29)]
    [StringLength(8)]
    public String? PaymentType2AuthDate { get; set; }
    [Index(30)]
    [StringLength(9)]
    public String? PaymentType2Total { get; set; }

    [Index(31)]
    public String? PaymentType3 { get; set; }
    [Index(32)]
    public String? PaymentType3Num { get; set; }
    [Index(33)]
    public String? PaymentType3Expires { get; set; }
    [Index(34)]
    public String? PaymentType3AuthCode { get; set; }
    [Index(35)]
    public String? PaymentType3AuthDate { get; set; }
    [Index(36)]
    public String? PaymentType3Total { get; set; }

    [Index(37)]
    public String? VendorCode { get; set; }

    [Index(38)]
    [StringLength(9)]
    public String? ItemReferenceNum { get; set; }
    [Index(39)]
    [StringLength(10)]
    public String? ItemId { get; set; }
    [Index(40)]
    [StringLength(30)]
    public String? ItemDescription { get; set; }
    [Index(41)]
    [StringLength(10)]
    public String? Personalization { get; set; }
    [Index(42)]
    [StringLength(9)]
    [Constant("0.00")]
    public String? PersonalizationCharge { get; set; }
    [Index(43)]
    public String? DecrementingLocation { get; set; }
    [Index(44)]
    public String? ItemColor { get; set; }

    [Index(45)]
    [StringLength(10)]
    [Constant("WEB")]
    public String? OrderType { get; set; }

    [Index(46)]
    [StringLength(9)]
    public String? ItemPrice { get; set; }
    [Index(47)]
    [StringLength(9)]
    public String? ItemSalesTax { get; set; }
    [Index(48)]
    [StringLength(9)]
    [Constant("0")]
    public String? ItemShipCharge { get; set; }
    [Index(49)]
    [StringLength(6)]
    public String? ItemQuantity { get; set; }
    [Index(50)]
    [StringLength(10)]
    public String? ItemShipMethod { get; set; }


    [Index(51)]
    [StringLength(20)]
    public String? ShipToFirstName { get; set; }
    [Index(52)]
    [StringLength(30)]
    public String? ShipToLastName { get; set; }
    [Index(53)]
    [StringLength(30)]
    public String? ShipToAddress1 { get; set; }
    [Index(54)]
    [StringLength(30)]
    public String? ShipToAddress2 { get; set; }
    [Index(55)]
    [StringLength(30)]
    public String? ShipToCity { get; set; }
    [Index(56)]
    [StringLength(2)]
    public String? ShipToState { get; set; }
    [Index(57)]
    [StringLength(10)]
    public String? ShipToZip { get; set; }
    [Index(58)]
    [StringLength(10)]
    public String? ShipToCountry { get; set; }
    [Index(59)]
    [StringLength(14)]
    public String? ShipToTelephone { get; set; }
    [Index(60)]
    [StringLength(60)]
    public String? ShipToEmail { get; set; }

    [Index(61)]
    public String? ActionFlag { get; set; }
    [Index(62)]
    public String? CustomerNum { get; set; }
    [Index(63)]
    [StringLength(1)]
    [Constant("N")]
    public String? OptOut { get; set; }
    [Index(64)]
    [StringLength(9)]
    public String? ShipTax { get; set; }
    [Index(65)]
    [StringLength(10)]
    public String? SalesPromotion { get; set; }
    [Index(66)]
    public String? ActualShippingCharges { get; set; }
    [Index(67)]
    public String? ActualShippingWeight { get; set; }
    [Index(68)]
    public String? ShipDate { get; set; }
    [Index(69)]
    public String? ItemShipMethod1 { get; set; }
    [Index(70)]
    public String? PkgTrackingNumber { get; set; }
    [Index(71)]
    public String? ReturnDate { get; set; }
}

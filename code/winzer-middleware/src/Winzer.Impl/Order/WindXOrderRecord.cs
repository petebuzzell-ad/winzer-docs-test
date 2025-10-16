using CsvHelper.Configuration.Attributes;

namespace Winzer.Impl.Order;

public class WindXOrderRecord
{
    [Name("Order #")]
    public String? OrderNumber { get; set; }
    [Name("Ship To First Name")]
    public String? ShipToFirstName { get; set; }
    [Name("Ship To Last Name")]
    public String? ShipToLastName { get; set; }
    [Name("Ship To Address 1")]
    public String? ShipToAddress1 { get; set; }
    [Name("Ship To Address 2")]
    public String? ShipToAddress2 { get; set; }
    [Name("Ship To City")]
    public String? ShipToCity { get; set; }
    [Name("Ship To State")]
    public String? ShipToState { get; set; }
    [Name("Ship To Zip")]
    public String? ShipToZip { get; set; }
    [Name("Ship To Phone")]
    public String? ShipToPhone { get; set; }
    [Name("Bill To First Name")]
    public String? BillToFirstName { get; set; }
    [Name("Bill To Last Name")]
    public String? BillToLastName { get; set; }
    [Name("Bill To Address 1")]
    public String? BillToAddress1 { get; set; }
    [Name("Bill To Address 2")]
    public String? BillToAddress2 { get; set; }
    [Name("Bill To City")]
    public String? BillToCity { get; set; }
    [Name("Bill To State")]
    public String? BillToState { get; set; }
    [Name("Bill To Zip")]
    public String? BillToZip { get; set; }
    [Name("Bill To Phone")]
    public String? BillToPhone { get; set; }
    [Name("Customer Id")]
    public String? CustomerId { get; set; }
    [Name("Payment Method")]
    public String? PaymentMethod { get; set; }
    [Name("Last Four")]
    public String? LastFour { get; set; }
    [Name("Total")]
    public String? Total { get; set; }
    [Name("Coupon Name")]
    public String? CouponName { get; set; }
    [Name("Ship Method")]
    public String? ShipMethod { get; set; }
    [Name("UPC")]
    public String? Upc { get; set; }
    [Name("Style Number")]
    public String? StyleNumber { get; set; }
    [Name("Size")]
    public String? Size { get; set; }
    [Name("Color")]
    public String? Color { get; set; }
    [Name("Unit Price")]
    public String? UnitPrice { get; set; }
    [Name("Sold For")]
    public String? SoldFor { get; set; }
    [Name("Qty")]
    public String? Qty { get; set; }
    [Name("Product Name")]
    public String? ProductName { get; set; }
    [Name("Carrier Requested")]
    [Constant("BEST")]
    public String? CarrierRequested { get; set; }
    [Name("Shipping Cost")]
    public String? ShippingCost { get; set; }
    [Name("Gift Message")]
    public String? GiftMessage { get; set; }
    [Name("Email")]
    public String? Email { get; set; }
    [Name("Order Date")]
    public String? OrderDate { get; set; }
    [Name("Order Time")]
    public String? OrderTime { get; set; }
    [Name("Tax")]
    public String? Tax { get; set; }
    [Name("Order Source")]
    [Constant("Shopify")]
    public String? OrderSource { get; set; }
    [Name("Gift Card Total")]
    public String? GiftCardTotal { get; set; }
    [Name("Charmbuild")]
    public String? Charmbuild { get; set; }
    [Name("Gift Wrap")]
    public String? GiftWrap { get; set; }
    [Name("Gem Comment")]
    public String? GemComment { get; set; }
    [Name("Skip1")]
    public String? Skip1 { get; set; }
    [Name("Skip2")]
    public String? Skip2 { get; set; }
    [Name("Skip3")]
    public String? Skip3 { get; set; }
    [Name("Gift Card Info")]
    public String? GiftCardInfo { get; set; }
    [Name("Skip4")]
    public String? Skip4 { get; set; }
    [Name("Skip5")]
    public String? Skip5 { get; set; }
    [Name("Skip6")]
    public String? Skip6 { get; set; }
    [Name("Skip7")]
    public String? Skip7 { get; set; }
    [Name("Skip8")]
    public String? Skip8 { get; set; }
    [Name("Transnumber")]
    public String? Transnumber { get; set; }
}

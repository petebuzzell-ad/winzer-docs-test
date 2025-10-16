namespace Winzer.Library.Fulfillment;

public class FulfillmentRecord
{
    public string TrackingNumber { get; set; } = String.Empty;
    public string OrderId { get; set; } = String.Empty;
    public string StyleNumbers { get; set; } = String.Empty;
    public string Carrier { get; set; } = String.Empty;
}

using Cql.Middleware.Library.Shopify.Order;

namespace Cql.Middleware.Library.Shopify.Shipping
{
    public interface IFulfillmentService
    {
        public Task<ShopifyFulfillment?> CreateFulfillment(FulfillmmentInput input, CancellationToken cancellationToken = default);
    }

    public class FulfillmentOptions : OrderOptions
    {
        public String ArchiveFolder { get; set; } = "archive";
        public String FulfillmentFolder { get; set; } = "shipments";
        public String ErrorFolder { get; set; } = "error";
        public int MaxFileSizeinBytes = 22528;
    }
}

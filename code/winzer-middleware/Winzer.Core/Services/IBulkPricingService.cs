using Winzer.Core.Types;

namespace Winzer.Core.Services
{
    public interface IBulkPricingService
    {
        BulkPricing CreateBulkPricing(BulkPricing bulkPricing);
        BulkPricing UpdateBulkPricing(BulkPricing bulkPricing);
        BulkPricing GetBulkPricing(string shopifyVariantID, int? quantity);
    }
}

using Winzer.Core.Types;

namespace Winzer.Core.Repo
{
    public interface IBulkPricingRepo
    {
        BulkPricing CreateBulkPricing(BulkPricing bulkPricing);
        BulkPricing UpdateBulkPricing(BulkPricing bulkPricing);
        BulkPricing? GetBulkPricing(string shopifyVariantID, int? quantity);
    }
}

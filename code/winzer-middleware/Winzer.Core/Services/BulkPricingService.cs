using Winzer.Core.Repo;
using Winzer.Core.Types;

namespace Winzer.Core.Services
{
    public class BulkPricingService : IBulkPricingService
    {
        private readonly IBulkPricingRepo _bulkPricingRepo;

        public BulkPricingService(IBulkPricingRepo bulkPricingRepo)
        {
            _bulkPricingRepo = bulkPricingRepo;
        }

        public BulkPricing CreateBulkPricing(BulkPricing bulkPricing)
        {

            bulkPricing.UtcCreated = DateTime.UtcNow;
            bulkPricing.UtcUpdated = DateTime.UtcNow;
            return _bulkPricingRepo.CreateBulkPricing(bulkPricing);
        }

        public BulkPricing GetBulkPricing(string shopifyVariantID, int? quantity)
        {
            return _bulkPricingRepo.GetBulkPricing(shopifyVariantID, quantity);
        }

        public BulkPricing UpdateBulkPricing(BulkPricing bulkPricing)
        {
            bulkPricing.UtcUpdated = DateTime.UtcNow;
            return _bulkPricingRepo.UpdateBulkPricing(bulkPricing);
        }
    }
}

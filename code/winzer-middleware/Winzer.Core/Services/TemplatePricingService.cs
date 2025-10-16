using Winzer.Core.Repo;
using Winzer.Core.Types;

namespace Winzer.Core.Services
{
    public class TemplatePricingService : ITemplatePricingService
    {
        private readonly ITemplatePricingRepo _templatePricingRepo;

        public TemplatePricingService(ITemplatePricingRepo templatePricingRepo)
        {
            _templatePricingRepo = templatePricingRepo;
        }

        public async Task MergeTemplatePricing(TemplatePricing item)
        {
            item.UtcCreated = item.UtcUpdated = DateTime.UtcNow;
            await _templatePricingRepo.MergeTemplatePricing(item);
        }
    }
}

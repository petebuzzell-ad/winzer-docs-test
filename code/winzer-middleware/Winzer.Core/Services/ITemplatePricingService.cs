using Winzer.Core.Types;

namespace Winzer.Core.Services
{
    public interface ITemplatePricingService
    {
        Task MergeTemplatePricing(TemplatePricing item);
    }
}

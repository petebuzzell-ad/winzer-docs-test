using Winzer.Core.Types;

namespace Winzer.Core.Repo
{
    public interface ITemplatePricingRepo
    {
        Task MergeTemplatePricing(TemplatePricing item);
    }
}

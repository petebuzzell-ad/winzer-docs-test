using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winzer.Core.Repo;
using Winzer.Core.Types;

namespace Winzer.Core.Services
{
    public class LastPurchasePricingService : ILastPurchasePricingService
    {
        private readonly ILastPurchasePricingRepo _repo;

        public LastPurchasePricingService(ILastPurchasePricingRepo repo)
        {
            _repo = repo;
        }

        public async Task MergeLastPurchasePricing(LastPurchasePricing lastPurchasePricing)
        {
            lastPurchasePricing.UtcCreated = lastPurchasePricing.UtcUpdated = DateTime.UtcNow;
            await _repo.MergeLastPurchasePricing(lastPurchasePricing);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winzer.Core.Repo;
using Winzer.Core.Types;

namespace Winzer.Core.Services
{
    public class ContractPricingService : IContractPricingService
    {
        private readonly IContractPricingRepo _repo;

        public ContractPricingService(IContractPricingRepo repo)
        {
            _repo = repo;
        }

        public async Task MergeContractPricing(ContractPricing item)
        {
            item.UtcCreated = item.UtcUpdated = DateTime.UtcNow;
            await _repo.MergeContractPricing(item);
        }
    }
}

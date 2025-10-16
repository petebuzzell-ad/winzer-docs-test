using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winzer.Core.Types;

namespace Winzer.Core.Repo
{
    public interface ILastPurchasePricingRepo
    {
        Task MergeLastPurchasePricing(LastPurchasePricing item);
    }
}

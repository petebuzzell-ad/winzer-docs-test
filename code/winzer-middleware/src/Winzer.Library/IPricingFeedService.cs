using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winzer.Library
{
    public interface IPricingFeedService
    {
        Task<bool> ImportPricingData();

        /// <summary>
        /// WARNING WARNING WARNING - Use of this method is only for development purposes!
        /// </summary>
        /// <returns></returns>
        Task<bool> DeleteAllTheCatalogs();

        // This should just be a one-time thing.
        Task<bool> AddEverybodyToMillionCatalog();
    }

    public class PricingImportOptions
    {
        public String CompanyName { get; set; } = "Winzer";
        public bool UseCatalogs { get; set; } = true;
    }
}

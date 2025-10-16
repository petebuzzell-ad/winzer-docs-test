using Microsoft.Extensions.Options;
using Winzer.Core.Repo;
using Winzer.Core.Types;

namespace Winzer.Repo
{
    public class BulkPricingRepo : RepoBase, IBulkPricingRepo
    {
        public BulkPricingRepo(IOptionsMonitor<RepoOptions> options) : base(options)
        {
        }

        public BulkPricing CreateBulkPricing(BulkPricing bulkPricing)
        {
            using var context = GetWinzerDBContext();

            context.BulkPricing.Add(bulkPricing);

            context.SaveChanges();

            return bulkPricing;
        }

        public BulkPricing UpdateBulkPricing(BulkPricing bulkPricing)
        {
            using var context = GetWinzerDBContext();

            bulkPricing.UtcUpdated = DateTime.UtcNow;
            context.BulkPricing.Update(bulkPricing);

            context.SaveChanges();

            return bulkPricing;
        }

        public BulkPricing? GetBulkPricing(string shopifyVariantID, int? quantity)
        {
            using var context = GetWinzerDBContext();

            var bulkPricing = context.BulkPricing
                .SingleOrDefault(p => p.ShopifyVariantID == shopifyVariantID
                && p.quantity == quantity);

            return bulkPricing;
        }
    }
}

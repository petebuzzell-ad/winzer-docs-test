using Microsoft.Extensions.Options;
using Winzer.Core.Repo;
using Winzer.Core.Types;

namespace Winzer.Repo
{
    public class TemplatePricingRepo : RepoBase, ITemplatePricingRepo
    {
        public TemplatePricingRepo(IOptionsMonitor<RepoOptions> options) : base(options)
        {
        }

        public async Task MergeTemplatePricing(TemplatePricing item)
        {
            var sql = @"
INSERT INTO template_pricing (template_name, brand_id, shopify_product_id, shopify_variant_id, template_price, utc_created_at, utc_updated_at)
VALUES (@0, CAST(@1 as ""brand_enum""), @2, @3, @4, @5, @6)
ON CONFLICT (brand_id, template_name, shopify_variant_id) DO UPDATE SET
    template_price = @4, utc_updated_at = @6
";
            var db = GetDatabase();
            await db.ExecuteAsync(sql, item.TemplateName, item.BrandID.ToString(), item.ShopifyProductID, item.ShopifyVariantID, item.TemplatePrice, item.UtcCreated, item.UtcUpdated);
        }
    }
}

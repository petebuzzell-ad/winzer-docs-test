using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winzer.Core.Repo;
using Winzer.Core.Types;

namespace Winzer.Repo
{
    public class LastPurchasePricingRepo : RepoBase, ILastPurchasePricingRepo
    {
        public LastPurchasePricingRepo(IOptionsMonitor<RepoOptions> options) : base(options)
        {
        }

        public async Task MergeLastPurchasePricing(LastPurchasePricing item)
        {
            var sql = @"
INSERT INTO last_purchase_pricing (shopify_company_id, brand_id, shopify_product_id, shopify_variant_id, last_purchase_price, utc_created_at, utc_updated_at)
VALUES (@0, CAST(@1 as ""brand_enum""), @2, @3, @4, @5, @6)
ON CONFLICT (brand_id, shopify_company_id, shopify_variant_id) DO UPDATE SET
    last_purchase_price = @4, utc_updated_at = @6
";
            var db = GetDatabase();
            await db.ExecuteAsync(sql, item.ShopifyCompanyID, item.BrandID.ToString(), item.ShopifyProductID, item.ShopifyVariantID, item.LastPurchasePrice, item.UtcCreated, item.UtcUpdated);
        }
    }
}

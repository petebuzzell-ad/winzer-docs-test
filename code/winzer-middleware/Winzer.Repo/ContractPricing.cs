using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winzer.Core.Repo;
using Winzer.Core.Types;

namespace Winzer.Repo
{
    public class ContractPricingRepo : RepoBase, IContractPricingRepo
    {
        public ContractPricingRepo(IOptionsMonitor<RepoOptions> options) : base(options)
        {
        }

        public async Task MergeContractPricing(ContractPricing item)
        {
            var sql = @"
INSERT INTO contract_pricing (contract_id, brand_id, shopify_product_id, shopify_variant_id, contract_price, utc_created_at, utc_updated_at)
VALUES (@0, CAST(@1 as ""brand_enum""), @2, @3, @4, @5, @6)
ON CONFLICT (brand_id, contract_id, shopify_variant_id) DO UPDATE SET
    contract_price = @4, utc_updated_at = @6
";
            var db = GetDatabase();
            await db.ExecuteAsync(sql, item.ContractID, item.BrandID.ToString(), item.ShopifyProductID, item.ShopifyVariantID, item.ContractPrice, item.UtcCreated, item.UtcUpdated);
        }
    }
}

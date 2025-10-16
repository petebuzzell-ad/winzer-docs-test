using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winzer.Core.Types
{
    [Table("last_purchase_pricing", Schema = "public")]
    public class LastPurchasePricing
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column("last_purchase_pricing_id")]
        public int LastPurchasePricingID { get; set; }
        [Column("brand_id")]
        public BrandEnum BrandID { get; set; }
        [Column("shopify_company_id")]
        public string ShopifyCompanyID { get; set; }
        [Column("shopify_product_id")]
        public string ShopifyProductID { get; set; }
        [Column("shopify_variant_id")]
        public string ShopifyVariantID { get; set; }
        [Column("last_purchase_price")]
        public decimal? LastPurchasePrice { get; set; }
        [Column("utc_created_at")]
        public DateTime UtcCreated { get; set; }
        [Column("utc_updated_at")]
        public DateTime UtcUpdated { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Winzer.Core.Types
{
    [Table("contract_pricing", Schema = "public")]
    public class ContractPricing
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column("contract_pricing_id")]
        public int ContractPricingID { get; set; }
        [Column("brand_id")]
        public BrandEnum BrandID { get; set; }
        [Column("contract_id")]
        public string ContractID { get; set; }
        [Column("shopify_product_id")]
        public string ShopifyProductID { get; set; }
        [Column("shopify_variant_id")]
        public string ShopifyVariantID { get; set; }
        [Column("contract_price")]
        public decimal? ContractPrice { get; set; }
        [Column("utc_created_at")]
        public DateTime UtcCreated { get; set; }
        [Column("utc_updated_at")]
        public DateTime UtcUpdated { get; set; }
    }
}

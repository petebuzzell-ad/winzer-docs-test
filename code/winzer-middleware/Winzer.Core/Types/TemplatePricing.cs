using System.ComponentModel.DataAnnotations.Schema;

namespace Winzer.Core.Types
{
    [Table("template_pricing", Schema = "public")]
    public class TemplatePricing
    { 
        [System.ComponentModel.DataAnnotations.Key]
        [Column("template_pricing_id")]
        public int TemplatePricingID { get; set; }
        [Column("brand_id")]
        public BrandEnum BrandID { get; set; }
        [Column("template_name")]
        public string TemplateName { get; set; }
        [Column("shopify_product_id")]
        public string ShopifyProductID { get; set; }
        [Column("shopify_variant_id")]
        public string ShopifyVariantID { get; set; }
        [Column("template_price")]
        public decimal? TemplatePrice { get; set; }
        [Column("utc_created_at")]
        public DateTime UtcCreated { get; set; }
        [Column("utc_updated_at")]
        public DateTime UtcUpdated { get; set; }
    }
}

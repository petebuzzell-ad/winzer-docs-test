using System.ComponentModel.DataAnnotations.Schema;

namespace Winzer.Core.Types
{
    [Table("bulk_pricing", Schema = "public")]
    public class BulkPricing
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column("bulk_pricing_id")]
        public int BulkPricingID { get; set; }
        [Column("shopify_product_id")]
        public string ShopifyProductID { get; set; }
        [Column("shopify_variant_id")]
        public string ShopifyVariantID { get; set; }
        [Column("quantity")]
        public int? quantity { get; set; }
        [Column("bulk_price")]
        public decimal? BulkPrice { get; set; }
        [Column("utc_created_at")]
        public DateTime UtcCreated { get; set; }
        [Column("utc_updated_at")]
        public DateTime UtcUpdated { get; set; }
    }
}

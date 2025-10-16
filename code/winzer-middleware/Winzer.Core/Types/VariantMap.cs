using System.ComponentModel.DataAnnotations.Schema;

namespace Winzer.Core.Types
{
    [Table("variant_map", Schema = "public")]
    public class VariantMap
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column("variant_map_id")]
        public int ProductMapID { get; set; }
        [Column("shopify_product_id")]
        public string? ShopifyProductID { get; set; }
        [Column("shopify_variant_id")]
        public string? ShopifyVariantID { get; set; }
        [Column("oracle_id")]
        public string? OracleID { get; set; }
        [Column("utc_created_at")]
        public DateTime UtcCreated { get; set; }
        [Column("utc_updated_at")]
        public DateTime UtcUpdated { get; set; }
        [Column("brand_id")]
        public BrandEnum BrandId { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Winzer.Core.Types
{
    [Table("product_map", Schema = "public")]
    public class ProductMap
    {
        [System.ComponentModel.DataAnnotations.Key]
        [Column("product_map_id")]
        public int ProductMapID { get; set; }
        [Column("shopify_id")]
        public string? ShopifyID { get; set; }
        [Column("oracle_id")]
        public string? OracleID { get; set; }
        [Column("product_hash")]
        public string? ProductHash { get; set; }
        [Column("shopify_product_handle")]
        public string? ShopifyProductHandle { get; set; }
        [Column("utc_created_at")]
        public DateTime UtcCreated { get; set; }
        [Column("utc_updated_at")]
        public DateTime UtcUpdated { get; set; }
        [Column("brand_id")]
        public BrandEnum BrandId { get; set; }
    }
}

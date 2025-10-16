using Cql.Middleware.Library.Shopify.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Shopify.Products
{
    public class ShopifyProduct
    {
        public string? Id { get; set; }

        public string? Title { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public DateTimeOffset? PublishedAt { get; set; }

        public string? Vendor { get; set; }

        public string? ProductType { get; set; }

        public string? Handle { get; set; }

        public string? TemplateSuffix { get; set; }

        public string? PublishedScope { get; set; }

        public string? Tags { get; set; }

        public string? Status { get; set; }

        public string? DescriptionHtml { get; set; }

        public IList<ShopifyProductVariant> Variants { get; set; } = new List<ShopifyProductVariant>();

        public IList<ShopifyProductOption> Options { get; set; } = new List<ShopifyProductOption>();

        public IList<ShopifyProductImage> Images { get; set; } = new List<ShopifyProductImage>(); 

        public IList<ShopifyMetaField> MetaFields { get; set; } = new List<ShopifyMetaField>(); 
    }
}

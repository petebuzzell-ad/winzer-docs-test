using Cql.Middleware.Library.Shopify.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Shopify.Products
{
    public class ShopifyProductVariant
    {
        public ShopifyProductVariant()
        {
            Metafields = new List<ShopifyMetaField>();
        }

        public string? Id { get; set; }

        public string? ProductId
        {
            get;
            set;
        }

        public string? Title
        {
            get;
            set;
        }

        public string? SKU
        {
            get;
            set;
        }

        public int? Position
        {
            get;
            set;
        }

        public long? Grams
        {
            get;
            set;
        }

        public string? InventoryPolicy
        {
            get;
            set;
        }

        public string? FulfillmentService
        {
            get;
            set;
        }

        public long? InventoryItemId
        {
            get;
            set;
        }

        public string? InventoryManagement
        {
            get;
            set;
        }

        public decimal? Price
        {
            get;
            set;
        }

        public decimal? CompareAtPrice
        {
            get;
            set;
        }

        public string? Option1
        {
            get;
            set;
        }

        public string? Option2
        {
            get;
            set;
        }

        public string? Option3
        {
            get;
            set;
        }

        public DateTimeOffset? CreatedAt
        {
            get;
            set;
        }

        public DateTimeOffset? UpdatedAt
        {
            get;
            set;
        }

        public bool? Taxable
        {
            get;
            set;
        }

        public string? TaxCode
        {
            get;
            set;
        }

        public bool? RequiresShipping
        {
            get;
            set;
        }

        public string? Barcode
        {
            get;
            set;
        }

        public long? InventoryQuantity
        {
            get;
            set;
        }

        public string? MediaId
        {
            get;
            set;
        }

        public string? MediaSrc { get; set; }

        public string? MediaAlt { get; set; }

        public decimal? Weight
        {
            get;
            set;
        }

        public string? WeightUnit
        {
            get;
            set;
        }

        public ShopifyMetaField? Metafield
        {
            get;
            set;
        }

        public IList<ShopifyMetaField> Metafields
        {
            get;
            set;
        }

        public IList<ShopifyMetaField> NewMetafields
        {
            get;
            set;
        }

        public IList<ShopifyPresentmentPrice>? PresentmentPrices
        {
            get;
            set;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Shopify.PriceList
{
    public class ShopifyPriceList
    {
        public string? Id {  get; set; }

        public string? Name { get; set; }

        public string? CurrencyCode { get; set; }

        public string? CatalogId { get; set; }
    }
}

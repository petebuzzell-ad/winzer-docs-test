using Cql.Middleware.Library.Shopify.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Shopify.Products
{
    public class ShopifyPresentmentPrice
    {
        public Money? Price { get; set; }

        public Money? CompareAtPrice { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Shopify.Products
{
    public class ShopifyProductOptionValue
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    public class ShopifyProductOption
    {
        public ShopifyProductOption()
        {
        }

        public string? Id
        {
            get;
            set;
        }

        public string? Name
        {
            get;
            set;
        }

        public IList<ShopifyProductOptionValue>? Values
        {
            get;
            set;
        }
    }
}

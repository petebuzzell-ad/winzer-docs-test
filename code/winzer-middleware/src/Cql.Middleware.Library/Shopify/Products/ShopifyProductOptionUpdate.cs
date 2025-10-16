using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Shopify.Products
{
    public class ShopifyProductOptionUpdate
    {
        public string OptionId { get; set; }

        public string OptionName { get; set; }

        public IList<string> OptionValuesToAdd { get; set; }
    }
}

using Cql.Middleware.Library.Shopify.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winzer.Library.Salsify
{
    public interface ISalsifyProductTransmogrifier
    {
        /// <summary>
        /// Using dark magic, takes a product hierarchy from Salsify and transforms it into a Shopify product with variants.
        /// </summary>
        /// <param name="product">A Salsify Product</param>
        /// <returns>A Shopify Product</returns>
        ShopifyProduct TransmogrifyToShopifyProduct(SalsifyProduct product, List<string>? addOnProducts = null);
    }
}

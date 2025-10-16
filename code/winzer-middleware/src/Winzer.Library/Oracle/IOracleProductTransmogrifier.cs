using Cql.Middleware.Library.Shopify.Products;
using Winzer.Core.Types;

namespace Winzer.Library.Oracle
{
    public  interface IOracleProductTransmogrifier
    {
        /// <summary>
        /// Using dark magic, takes a product hierarchy from Salsify and transforms it into a Shopify product with variants.
        /// </summary>
        /// <param name="product">A Salsify Product</param>
        /// <returns>A Shopify Product</returns>
        ShopifyProduct TransmogrifyToShopifyProduct(OracleCSVRecord product, BrandEnum brandId);
    }
}

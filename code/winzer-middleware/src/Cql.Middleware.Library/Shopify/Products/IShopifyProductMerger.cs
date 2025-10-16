using Cql.Middleware.Library.Shopify.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Shopify.Products
{
    public interface IShopifyProductMerger
    {

        /// <summary>
        /// Takes the shopify product in 'original' and merges any changes in 'updated' into it.
        /// This is built under the assumption that 'original' is the product that you got from the Shopify API
        /// and that 'updated' came from Salsify or some other source.
        /// </summary>
        /// <param name="original">The product as it exists in Shopify</param>
        /// <param name="updated">The updated version of the product.</param>
        /// <returns>A new product with the changes in 'updated' merged in with 'original'.</returns>
        ShopifyProductWithDeletes MergeProducts(ShopifyProduct original, ShopifyProduct updated);
    }

    public class ShopifyProductWithDeletes : ShopifyProduct
    {
        public IList<string> MediaIdsToDelete { get; set; } = new List<string>();
        public IList<string> MetafieldIdsToDelete { get; set; } = new List<string>();

        public IList<ShopifyProductOptionUpdate> OptionUpdates { get; set; } = new List<ShopifyProductOptionUpdate>();
    }

    public class ShopifyProductVariantWithDeletes : ShopifyProductVariant
    {
        public IList<string> MetafieldIdsToDelete { get; set; } = new List<string>();
    }
}

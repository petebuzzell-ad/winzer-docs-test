using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Shopify.PriceList
{
    public interface IPriceListService
    {
        Task<List<ShopifyPriceListItem>> GetPriceListItems(string priceListId, string? cursor = null);

        Task<string?> CreatePriceList(ShopifyPriceList list);

        Task UpsertVariantsToPriceList(string priceListId, string currencyCode, IEnumerable<Tuple<string,decimal>> variantsAndPrices);
    }
}

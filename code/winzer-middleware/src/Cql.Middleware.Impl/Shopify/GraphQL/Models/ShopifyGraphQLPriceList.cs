using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Impl.Shopify.GraphQL.Models
{
    public class ShopifyGraphQLPriceListItemQueryResultData
    {
        public ShopifyGraphQLPriceList priceList { get; set; }
    }

    public class ShopifyGraphQLPriceListItemVariant
    {
        public string id { get; set; }
    }

    public class ShopifyGraphQLPriceListItem
    {
        public ShopifyGraphQLPriceListItemVariant variant { get; set; }
    }

    public class ShopifyGraphQLPriceList
    {
        public string? id {  get; set; }

        public string? name { get; set; }

        public string? currency { get; set; }

        public ShopifyGraphQLCatalog? catalog { get; set; }

        public ShopifyGraphQLResultPage<ShopifyGraphQLPriceListItem>? prices { get; set; }
    }
    public class ShopifyGraphQLPriceListQueryResultData
    {
        public ShopifyGraphQLResultPage<ShopifyGraphQLPriceList>? pricelists { get; set; }
    }
    public class ShopifyGraphQLPriceListUpsertResult
    {
        public ShopifyGraphQLPriceListUpsertResultData? priceListFixedPricesAdd { get; set; }

        public bool IsSuccessful()
        {
            if (priceListFixedPricesAdd.userErrors != null && priceListFixedPricesAdd.userErrors.Any()) return false;

            return true;
        }
    }

    public class ShopifyGraphQLPriceListUpsertResultData
    {
        public IEnumerable<ShopifyGraphQLUserError>? userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLCreatePriceListResult
    {
        public ShopifyGraphQLCreatePriceListData? priceListCreate { get; set; }

        public bool IsSuccessful()
        {
            if (priceListCreate == null || priceListCreate.priceList == null || string.IsNullOrWhiteSpace(priceListCreate.priceList.id)) return false;

            if (priceListCreate.userErrors != null && priceListCreate.userErrors.Any()) return false;

            return true;
        }
    }

    public class ShopifyGraphQLCreatePriceListData
    {
        public ShopifyGraphQLPriceList? priceList { get; set; }
        public IEnumerable<ShopifyGraphQLUserError>? userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }
}

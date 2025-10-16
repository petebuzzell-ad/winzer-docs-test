using Cql.Middleware.Impl.Shopify.GraphQL.Models;
using Cql.Middleware.Library.Shopify;
using Cql.Middleware.Library.Shopify.PriceList;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cql.Middleware.Impl.Shopify.Services
{
    public class PriceListService : BaseService, IPriceListService
    {
        public PriceListService(ILogger<PriceListService> logger, ShopifyGraphQLOptions options) : base(logger, options)
        {

        }

        public async Task<List<ShopifyPriceListItem>> GetPriceListItems(string priceListId, string? cursor = null)
        {
            var query = @"
query getPriceList($id: ID!, $after: String) {
    priceList(id:$id) {
        id
        name
        prices(first: 250, after: $after) {
            edges {
                node {
                    variant {
                        id
                    }
                }
            }
            pageInfo {
                ...pageInfoFields
            }
        }
    }
}

fragment pageInfoFields on PageInfo {
    hasNextPage
    endCursor    
}  
";

            var rval = new List<ShopifyPriceListItem>();
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLPriceListItemQueryResultData>(query, new { id = priceListId, after = cursor }, true);
            foreach (var item in result?.priceList?.prices?.edges)
            {
                rval.Add(new ShopifyPriceListItem()
                {
                    VariantId = item.node.variant.id
                });
            }

            if (result.priceList.prices.pageInfo.hasNextPage)
            {
                rval.AddRange(await GetPriceListItems(priceListId, result.priceList.prices.pageInfo.endCursor));
            }

            return rval;
        }

        public async Task UpsertVariantsToPriceList(string priceListId, string currencyCode, IEnumerable<Tuple<string, decimal>> variantsAndPrices)
        {
            var chunks = variantsAndPrices.Chunk(200);
            foreach ( var chunk in chunks )
            {
                await UpsertVariantsToPriceList_Internal(priceListId, currencyCode, chunk);
            }
        }

        public async Task UpsertVariantsToPriceList_Internal(string priceListId, string currencyCode, IEnumerable<Tuple<string, decimal>> variantsAndPrices)
        {
            var input = new
            {
                priceListId = priceListId,
                prices = variantsAndPrices.Select(x =>
                new
                {
                    variantId =  x.Item1, 
                    price = new
                    {
                        amount = x.Item2,
                        currencyCode = currencyCode,
                    }
                }).ToArray()
            };

            var query = @"
mutation priceListFixedPricesAdd($priceListId: ID!, $prices: [PriceListPriceInput!]!) {
    priceListFixedPricesAdd(priceListId: $priceListId, prices: $prices) {
        userErrors {
            field
            code
            message
        }
    }
}
";
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLPriceListUpsertResult>(query, input);
            if (result == null || !result.IsSuccessful())
            {
                _logger.LogError("Error adding items to price list with id {0}.  Details below if available.", priceListId);
                if (result?.priceListFixedPricesAdd?.userErrors?.Any() ?? false)
                {
                    foreach (var userError in result.priceListFixedPricesAdd.userErrors)
                    {
                        _logger.LogError("{0} - {1}", userError.field?.FirstOrDefault(), userError.message);
                    }
                }

                throw new Exception(string.Format("Failed to add items to price list with id {0}.  See earlier log messages for details.", priceListId));
            }
        }

        public async Task<string?> CreatePriceList(ShopifyPriceList list)
        {
            var input = new
            {
                catalogId = list.CatalogId,
                name = list.Name,
                currency = list.CurrencyCode,
                parent = new {
                    adjustment = new
                    {
                        type = "PERCENTAGE_INCREASE",
                        value = 0,
                    }
                }
            };

            var query = @"
mutation PriceListCreate($input: PriceListCreateInput!) {
    priceListCreate(input: $input) {
        priceList {
            id
        }
        userErrors {
            field
            message
        }
    }
}
";

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLCreatePriceListResult>(query, new { input }, false);
            if (result == null || !result.IsSuccessful())
            {
                _logger.LogError("Error creating price list with name {0}.  Details below if available.", list.Name);
                if (result?.priceListCreate?.userErrors?.Any() ?? false)
                {
                    foreach (var userError in result.priceListCreate.userErrors)
                    {
                        _logger.LogError("{0} - {1}", userError.field?.FirstOrDefault(), userError.message);
                    }
                }

                return null;
            }
            else
            {
                return result.priceListCreate.priceList.id;
            }

        }
    }
}

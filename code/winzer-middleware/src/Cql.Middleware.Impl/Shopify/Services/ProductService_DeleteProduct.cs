using Cql.Middleware.Impl.Shopify.GraphQL.Models;
using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Products;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Dasync.Collections;

namespace Cql.Middleware.Impl.Shopify.Services
{
    public partial class ProductService
    {
        public async Task DeleteProducts(IEnumerable<string> products)
        {
            var query = @"
mutation productDelete($input: ProductDeleteInput!) {
    productDelete(input: $input) {
        userErrors {
            field
            message
        }
    }
}
";
            foreach (var shopifyProductId in products)
            {
                var parameters = new
                {
                    input = new
                    {
                        id = shopifyProductId,
                        synchronous = false,
                    },
                    synchronous = false
                };

                var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductDeleteResult>(query, parameters, true);
                if (!result.IsSuccessful())
                {
                    var errors = result.ErrorList();
                    if (errors.Any())
                    {
                        var firstError = errors.First();
                        _logger.LogError($"Product deletion for product '{shopifyProductId}' was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message}.");
                    }
                    else
                    {
                        _logger.LogError($"Product deletion for product '{shopifyProductId}' was unsuccessful.  An unexpected or invalid response received from Shopify.");
                    }
                }
            }
        }

        public async Task DeleteProduct(string shopifyProductId)
        {
            var query = @"
mutation productDelete($input: ProductDeleteInput!) {
    productDelete(input: $input) {
        userErrors {
            field
            message
        }
    }
}
";

            var parameters = new
            {
                input = new
                {
                    id = shopifyProductId,
                },
                synchronous = false
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductDeleteResult>(query, parameters, true);
            if (!result.IsSuccessful())
            {
                var errors = result.ErrorList();
                if (errors.Any())
                {
                    var firstError = errors.First();
                    throw new Exception($"Product deletion for product '{shopifyProductId}' was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message}.");
                }
                else
                {
                    throw new Exception($"Product deletion for product '{shopifyProductId}' was unsuccessful.  An unexpected or invalid response received from Shopify.");
                }
            }
        }
    }
}

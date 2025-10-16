using Cql.Middleware.Impl.Shopify.GraphQL.Models;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Impl.Shopify.Services
{
    public partial class ProductService
    {
        public async Task ReOrderProductVariants(string productId, IList<string> variantsInOrder)
        {
            if (!productId.StartsWith("gid"))
            {
                productId = $"gid://shopify/Product/{productId}";
            }

            var query = @"
mutation productVariantsBulkReorder($positions: [ProductVariantPositionInput!]!, $productId: ID!) {
    productVariantsBulkReorder(positions: $positions, productId: $productId) {
        userErrors {
            field
            message
        }
    }
}
";

            var positions = new List<ShopifyGraphQLProductVariantPositionInput>();
            int position = 1;
            foreach ( var variantId in variantsInOrder )
            {
                var id = variantId;
                if (!id.StartsWith("gid"))
                {
                    id = $"gid://shopify/ProductVariant/{variantId}";
                }

                positions.Add(new ShopifyGraphQLProductVariantPositionInput(id, position));
                position++;
            }

            var parameters = new
            {
                positions = positions,
                productId = productId,
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductVariantsBulkReorderResult>(query, parameters, true);
            if (!result.IsSuccessful())
            {
                var errors = result.ErrorList();
                if (errors.Any())
                {
                    var firstError = errors.First();
                    throw new Exception($"Product variant reordering was unsuccessful.  First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message}.  Note that due to Shopify bugs, Shopify may have updated the product anyways.");
                }
                else
                {
                    throw new Exception("Product variant reordering was unsuccessful.  An unexpected or invalid response received from Shopify.");
                }
            }
        }
    }
}

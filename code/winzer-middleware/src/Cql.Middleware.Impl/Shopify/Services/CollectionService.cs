using System.Text;
using Microsoft.Extensions.Logging;
using GraphQL.Client.Abstractions;

using Cql.Middleware.Library.Shopify;
using Cql.Middleware.Library.Shopify.Collection;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using Dasync.Collections;
using Cql.Middleware.Library.Shopify.Products;
using System.Collections.Concurrent;

namespace Cql.Middleware.Impl.Shopify.Services
{
    public class CollectionService : BaseService, ICollectionService
    {
        public CollectionService(ILogger<InventoryService> logger, ShopifyGraphQLOptions options)
            : base(logger, options)
        {
            // _inventoryOptions = inventoryOptions;
        }

        public async Task<ShopifyCollectionsQueryResult?> GetAllCollections(string cursor = "")
        {
            var afterCursor = "";
            if (!string.IsNullOrWhiteSpace(cursor))
            {
                afterCursor = $"after:\"{cursor}\",";
            }
            var collectionQuery = $@"
query collections($query: String!) {{
    collections(first: 200, {afterCursor} query: $query) {{
        edges {{
            node {{
                id
                title
            }}
        }}
        pageInfo {{
            hasNextPage
            endCursor
        }}
    }}
}}";

            var queryParameters = new
            {
                query = "collection_type:smart"
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLCollectionsQueryResultData>(collectionQuery, queryParameters);
            var collections = new ConcurrentBag<CollectionItem>();
            var allCollections = new List<CollectionItem>();

            if (result.collections != null && result.collections.edges != null)
            {
                await result.collections.edges.ParallelForEachAsync(async collection => {
                    if (collection.node != null)
                    {
                        var collectionItem = new CollectionItem()
                        {
                            id = collection.node.Id,
                            title = collection.node.Title,
                            handle = collection.node.Handle
                        };

                        collections.Add(collectionItem);
                    }
                }, maxDegreeOfParallelism: 1);
                allCollections = collections.ToList();

                if (result.collections.pageInfo.hasNextPage && !string.IsNullOrWhiteSpace(result.collections.pageInfo.endCursor))
                {
                    var endCursor = result.collections.pageInfo.endCursor;
                    var nextCollectionsPage = await GetAllCollections(endCursor);
                    allCollections = allCollections.Concat(nextCollectionsPage.Collections).ToList();
                }
            }

            var rval = new ShopifyCollectionsQueryResult()
            {
                HasMoreResults = result.collections?.pageInfo?.hasNextPage ?? false,
                Cursor = result.collections?.pageInfo?.endCursor,
                Collections = allCollections
            };

            return rval;
        }

        public async Task<ShopifyCollectionResult?> GetCollectionByHandle(string collectionHandle)
        {
            if (String.IsNullOrEmpty(collectionHandle)) { throw new ArgumentException("Collection Handle is required"); }
            var CollectionByHandleQuery = @"
query collectionByHandle(
  $collectionHandle: String!
) {
  collectionByHandle(handle: $collectionHandle) {
    id
    handle
  }
}";

            var queryParameters = new
            {
                collectionHandle = collectionHandle,
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLCollectionByHandleQueryResultData>(CollectionByHandleQuery, queryParameters);
            if (result.collectionByHandle != null)
            {
                return new ShopifyCollectionResult()
                {
                    Id = result.collectionByHandle.Id
                };
            }

            return null;
        }

        public async Task<ShopifyCollectionResult> CreateCollection(CollectionItem collection) {
            var createCollectionMutation = @"
mutation CollectionCreate(
  $input: CollectionInput!
) {
  collectionCreate(input: $input) {
    collection {
      id
      title
      descriptionHtml
      handle
      sortOrder
      ruleSet {
        appliedDisjunctively
        rules {
          column
          relation
          condition
        }
      }
    }
    userErrors {
      ...userErrorParts
    }
  }
}

fragment userErrorParts on UserError {
  field
  message
}";
            var input = new
            {
                title = collection.title,
                ruleSet = collection.ruleSet,
            };
            var queryParameters = new
            {
                input = input,
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLCollectionCreateResult>(createCollectionMutation, queryParameters);
            if (result.collectionCreate != null)
            {
                if (result.collectionCreate.userErrors.Any())
                {
                    var firstError = result.collectionCreate.userErrors.First();
                    throw new Exception($"Collection creation for collection '{collection.title}' was unsuccessful. First error was {string.Join(".", firstError.field ?? new string[] { "(no field)" })}: {firstError.message}.");
                }

                if (result.collectionCreate.collection != null)
                {
                    return new ShopifyCollectionResult()
                    {
                        Id = result.collectionCreate.collection.Id
                    };
                }
            }

            throw new Exception($"Collection creation for collection '{collection.title}' was unsuccessful. An unexpected or invalid response received from Shopify.");
        }

        public async Task<String?> GetCategoriesMetafield()
        {
            var CollectionByHandleQuery = @"
query metafield($ns: String!, $key: String!){
  metafieldDefinitions(first: 1, ownerType: PRODUCT, namespace: $ns, key: $key) {
    edges {
      node {
        name,
        id
      }
    }
  }
}";

            var queryParameters = new
            {
                ns = "cql",
                key = "categories"
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLMetafieldQueryResultData>(CollectionByHandleQuery, queryParameters);
            if (result.metafieldDefinitions?.edges?.Length > 0)
            {
                return result.metafieldDefinitions.edges[0].node?.id;
            }

            return null;
        }
    }
}

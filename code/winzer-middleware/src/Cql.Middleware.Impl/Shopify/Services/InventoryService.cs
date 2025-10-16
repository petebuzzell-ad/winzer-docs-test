using System.Text;
using Microsoft.Extensions.Logging;
using GraphQL.Client.Abstractions;

using Cql.Middleware.Library.Shopify;
using Cql.Middleware.Library.Shopify.Inventory;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using Dasync.Collections;

namespace Cql.Middleware.Impl.Shopify.Services
{
    public class InventoryService : BaseService, IInventoryService
    {
        protected readonly InventoryOptions _inventoryOptions;
        public InventoryService(ILogger<InventoryService> logger, ShopifyGraphQLOptions options, InventoryOptions inventoryOptions)
            : base(logger, options)
        {
            _inventoryOptions = inventoryOptions;
        }

        public async Task<IEnumerable<InventoryItem>> LookupInventory(ShopifyInventoryQuery query, CancellationToken cancellationToken = default)
        {
            if(String.IsNullOrEmpty(query.Sku)) { throw new ArgumentException("Sku is required"); }
            if(String.IsNullOrEmpty(query.LocationId)) { throw new ArgumentException("LocationId is required"); }
            var InventoryLookupQuery = @"
query InventoryLookup(
  $inventoryItemsQuery: String!
  $locationId: ID!
  $includeLocation: Boolean!
  $numInventoryItems: Int
  $after: String
) {
  inventoryItems(first: $numInventoryItems, query: $inventoryItemsQuery, after: $after) {
    pageInfo{
        ...pageInfoFragment
    }
    edges {
      node {
        ...inventoryItemFragment
        inventoryLevel(locationId: $locationId) {
          ...inventoryLevelFragment
          location @include(if: $includeLocation) {
            ...locationFragment
          }
        }
      }
    }
  }
}

fragment inventoryItemFragment on InventoryItem {
  ##INVENTORYITEM_PARTIAL##
}

fragment inventoryLevelFragment on InventoryLevel {
  ##INVENTORYLEVEL_PARTIAL##
}

fragment locationFragment on Location {
  activatable
  deactivatable
  deactivatedAt
  deletable
  fulfillsOnlineOrders
  hasActiveInventory
  hasUnfulfilledOrders
  id
  isActive
  legacyResourceId
  name
  shipsInventory
}

fragment pageInfoFragment on PageInfo {
  hasNextPage
  endCursor
}";

            var graphQL = new StringBuilder(InventoryLookupQuery);
            graphQL.Replace("##INVENTORYITEM_PARTIAL##", string.Join(Environment.NewLine, query.InventoryItemFields ?? new string[] { "id" }));
            graphQL.Replace("##INVENTORYLEVEL_PARTIAL##", string.Join(Environment.NewLine, query.InventoryLevelFields ?? new string[] { "id" }));

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLInventoryLookupResultData>(graphQL.ToString(), new {
                inventoryItemsQuery = $"sku:{query.Sku}",
                locationId = query.LocationId,
                includeLocation = false,
                numInventoryItems = query.NumInventoryItems,
                after = default(String?)
            }, cancellationToken: cancellationToken);

            var inventoryItems = result?.inventoryItems ?? new ShopifyGraphQLResultPage<ShopifyGraphQLInventoryItem>();
            if(inventoryItems.pageInfo.hasNextPage) {
                _logger.LogError("There are too many duplicate skus for {Sku}.  Consider increasing the ShopifyGraphQL:InventoryServiceOptions:NumInventoryItems - {NumInventoryItems}", query.Sku, query.NumInventoryItems);
            }
            var items = inventoryItems.ToList();
            if (!items.Any())
            {
                _logger.LogError("Could not find Invetory Item for {sku}", query.Sku);
            }
            return items.Select(i => i.Map());
        }

        public async IAsyncEnumerable<InventoryLevel> InventoryBulkLookup(ShopifyInventoryQuery query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if(String.IsNullOrEmpty(query.LocationId)) { throw new ArgumentException("LocationId is required"); }
            var InventoryLookupQuery = @"
query InventoryBulkLookup(
  $locationId: ID!
  $numInventoryLevels: Int!
  $after: String
) {
    location(id: $locationId) {
        inventoryLevels(first: $numInventoryLevels after: $after) {
            pageInfo{
                ...pageInfoFragment
            }
            edges {
                node {
                    ...inventoryLevelFragment
                    item {
                        ...inventoryItemFragment
                    }
                }
            }
        }
    }
}

fragment inventoryItemFragment on InventoryItem {
  ##INVENTORYITEM_PARTIAL##
}

fragment inventoryLevelFragment on InventoryLevel {
  ##INVENTORYLEVEL_PARTIAL##
}

fragment pageInfoFragment on PageInfo {
  hasNextPage
  endCursor
}";
            var graphQL = new StringBuilder(InventoryLookupQuery);
            graphQL.Replace("##INVENTORYITEM_PARTIAL##", string.Join(Environment.NewLine, query.InventoryItemFields ?? new string[] { "id" }));
            graphQL.Replace("##INVENTORYLEVEL_PARTIAL##", string.Join(Environment.NewLine, query.InventoryLevelFields ?? new string[] { "id" }));

            var after = default(String?);
            var hasNextPage = false;
            do
            {
                var result = await ExecuteGraphQLQuery<ShopifyGraphQLInventoryBulkLookupResultData>(graphQL.ToString(), new
                {
                    locationId = query.LocationId,
                    numInventoryLevels = query.NumBulkInventoryItems,
                    after = after
                }, cancellationToken: cancellationToken);

                var invLevels = result.location?.inventoryLevels ?? new ShopifyGraphQLResultPage<ShopifyGraphQLInventoryLevel>();
                hasNextPage = invLevels.pageInfo.hasNextPage;
                if (hasNextPage)
                {
                    after = invLevels.pageInfo.endCursor;
                }
                foreach (var inventoryLevel in invLevels.ToList())
                {
                    yield return inventoryLevel.Map();
                }
            } while (hasNextPage);
        }

        public async Task<int> InventoryBulkUpdate(IEnumerable<InventoryAdjustQuantity> items, CancellationToken cancellationToken = default)
        {
            var InventoryBulkUpdate = @"
mutation InventoryBulkUpdate(
  $inventoryItemAdjustments: [InventoryAdjustItemInput!]!
  $locationId: ID!
) {
  inventoryBulkAdjustQuantityAtLocation(
    inventoryItemAdjustments: $inventoryItemAdjustments
    locationId: $locationId
  ) {
    inventoryLevels {
      ...inventoryLevelFragment
    }
    userErrors {
      ...userErrorParts
    }
  }
}

fragment inventoryLevelFragment on InventoryLevel {
  id
}

fragment userErrorParts on UserError {
  field
  message
}";
            var locationItems = items.GroupBy((item) => item.LocationId);
            int count = 0;
            foreach (var locationGroup in locationItems)
            {
                var locationId = locationGroup.Key;
                await locationGroup.Chunk(_inventoryOptions.BulkUpdateChunkSize).ParallelForEachAsync(async itemsChunk => {
                    var result = await ExecuteGraphQLQuery<ShopifyGraphQLInventoryBulkUpdateResultData>(InventoryBulkUpdate, new {
                        inventoryItemAdjustments = itemsChunk.Select(l => new {
                            inventoryItemId = l.InventoryItemId,
                            availableDelta = l.AvailableDelta
                        }),
                        locationId = locationId,
                    }, cancellationToken: cancellationToken);
                    if(result.inventoryBulkAdjustQuantityAtLocation.userErrors.Any())
                    {
                        foreach(var userError in result.inventoryBulkAdjustQuantityAtLocation.userErrors)
                        {
                            _logger.LogError("Error on BulkAdjustQuantityAtLocation: {0} - {1} - {2}", locationId, userError.field?.FirstOrDefault(), userError.message);
                        }
                    }
                    count += result.inventoryBulkAdjustQuantityAtLocation.inventoryLevels.Count();
                });
            }
            return count;
        }

        public async Task<int> InventoryActivate(IEnumerable<InventoryAdjustQuantity> items, CancellationToken cancellationToken = default)
        {
            var inventoryActivate = @"
mutation InventoryActivate(
  $inventoryItemId: ID!
  $locationId: ID!
  $available: Int
) {
  inventoryActivate(
    inventoryItemId: $inventoryItemId
    locationId: $locationId
    available: $available
  ) {
    inventoryLevel {
      ...inventoryLevelFragment
    }
    userErrors {
      ...userErrorParts
    }
  }
}

fragment inventoryLevelFragment on InventoryLevel {
  id
}

fragment userErrorParts on UserError {
  field
  message
}";
            int count = 0;
            await items.ParallelForEachAsync(async item => {
                var result = await ExecuteGraphQLQuery<ShopifyGraphQLInventoryActivateResultData>(inventoryActivate, new {
                    inventoryItemId = item.InventoryItemId,
                    locationId = item.LocationId,
                    available = item.AvailableDelta
                }, cancellationToken: cancellationToken);
                if(result.inventoryActivate.userErrors.Any())
                {
                    foreach(var userError in result.inventoryActivate.userErrors)
                    {
                        _logger.LogError("Error on InventoryActivate: {0} - {1} - {2}", item.LocationId, userError.field?.FirstOrDefault(), userError.message);
                    }
                }
                count++;
            }, cancellationToken);
            return count;
        }
    }
}

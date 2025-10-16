using Microsoft.Extensions.Logging;
using GraphQL.Client.Abstractions;

using Cql.Middleware.Library.Shopify;
using Cql.Middleware.Library.Shopify.Shipping;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;

namespace Cql.Middleware.Impl.Shopify.Services;

public class FulfillmentService : BaseService, IFulfillmentService
{
    public FulfillmentService(ILogger<FulfillmentService> logger, ShopifyGraphQLOptions options)
        : base(logger, options) { }

    public async Task<ShopifyFulfillment?> CreateFulfillment(FulfillmmentInput input, CancellationToken cancellationToken = default)
    {
        var fulfillmentCreate = @"
mutation CreateFulfillment($fulfillment: FulfillmentV2Input!) {
  fulfillmentCreateV2(fulfillment: $fulfillment) {
    fulfillment {
      id
      fulfillmentLineItems(first: 10) {
        pageInfo {
          ...pageInfoFragment
        }
        edges {
          node {
            ...fulfillmentLineItemFragment
          }
        }
      }
      createdAt
      updatedAt
      }
    userErrors {
      ...userErrorParts
    }
  }
}

fragment userErrorParts on UserError {
  field
  message
}

fragment fulfillmentLineItemFragment on FulfillmentLineItem {
  id
  quantity
  lineItem {
    id
  }
}

fragment pageInfoFragment on PageInfo {
  hasNextPage
  endCursor
}
";
        var result = await ExecuteGraphQLQuery<ShopifyGraphQLFulfillmentCreateResultData>(fulfillmentCreate, new
        {
            fulfillment = new
            {
                lineItemsByFulfillmentOrder = input.LineItemsByFulfillmentOrder.Select(o => new
                {
                    fulfillmentOrderId = o.FulfillmentOrderId,
                    fulfillmentOrderLineItems = o.FulfillmentOrderLineItems.Select(i => new
                    {
                        id = i.Id,
                        quantity = i.Quantity
                    })
                }),
                notifyCustomer = input.NotifyCustomer,
                trackingInfo = new
                {
                    company = input.TrackingInfo?.Company,
                    number = input.TrackingInfo?.Number,
                    url = input.TrackingInfo?.Url,
                }
            },
        }, cancellationToken: cancellationToken);
        if (result.fulfillmentCreateV2.userErrors.Any())
        {
            foreach (var userError in result.fulfillmentCreateV2.userErrors)
            {
                _logger.LogError("Error on FulfillmentCreateV2: {0} - {1} - {2}", input.LineItemsByFulfillmentOrder.FirstOrDefault()?.FulfillmentOrderId, userError.field?.FirstOrDefault(), userError.message);
            }
        }
        return result.fulfillmentCreateV2.fulfillment != null ? await AssembleShopifyFulfillment(result.fulfillmentCreateV2.fulfillment, cancellationToken) : null;
    }

    private async Task<IList<ShopifyGraphQLFulfillmentLineItem>> GetAdditionalFulfillmentLineItems(string fulfillmentId, string? cursor = null, CancellationToken cancellationToken = default)
    {
        var graphQLQuery = @"
query FulfillmentLookup(
  $fulfillmentId: ID!
  $numLineItems: Int!
  $after: String
) {
  fulfillment(id:$fulfillmentId){
    id
    fulfillmentLineItems(first: $numLineItems, after: $after) {
      pageInfo {
        ...pageInfoFragment
      }
      edges {
        node {
          ...fulfillmentLineItemFragment
        }
      }
    }
    trackingInfo {
      ...fulfillmentTrackingInfoFragment
    }
    createdAt
    updatedAt
  }
}

fragment fulfillmentLineItemFragment on FulfillmentLineItem {
  id
  quantity
  lineItem {
    id
  }
}

fragment fulfillmentTrackingInfoFragment on FulfillmentTrackingInfo {
  number
  company
  url
}

fragment pageInfoFragment on PageInfo {
  hasNextPage
  endCursor
}
";

        var rval = new List<ShopifyGraphQLFulfillmentLineItem>();
        var after = cursor;
        var hasNextPage = false;
        do
        {
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLFulfillmentResultData>(graphQLQuery, new
            {
                fulfillmentId = fulfillmentId,
                numLineItems = 250,
                after = after
            }, cancellationToken: cancellationToken);

            var lineItems = result.fulfillment.fulfillmentLineItems ?? new ShopifyGraphQLResultPage<ShopifyGraphQLFulfillmentLineItem>();
            hasNextPage = lineItems.pageInfo.hasNextPage;
            if (hasNextPage)
            {
                after = lineItems.pageInfo.endCursor;
            }
            rval.AddRange(lineItems.ToList());

        } while (hasNextPage);

        return rval;
    }

    private async Task<ShopifyFulfillment> AssembleShopifyFulfillment(ShopifyGraphQLFulfillment node, CancellationToken cancellationToken = default)
    {
        if (!String.IsNullOrEmpty(node.id) && node.fulfillmentLineItems != null && node.fulfillmentLineItems.pageInfo.hasNextPage)
        {
            var additionalLineItems = await GetAdditionalFulfillmentLineItems(node.id, node.fulfillmentLineItems.pageInfo.endCursor, cancellationToken);
            if (additionalLineItems?.Any() ?? false)
            {
                var items = node.fulfillmentLineItems.ToList();
                items.AddRange(additionalLineItems);
                node.fulfillmentLineItems = new ShopifyGraphQLResultPage<ShopifyGraphQLFulfillmentLineItem>
                {
                    pageInfo = new ShopifyGraphQLPageInfo(),
                    edges = items.Select(i => new ShopifyGraphQLResultEdge<ShopifyGraphQLFulfillmentLineItem> { node = i }).ToArray()
                };
            }
        }

        return node.Map();
    }
}

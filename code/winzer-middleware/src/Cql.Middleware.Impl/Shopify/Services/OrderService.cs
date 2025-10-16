using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using GraphQL.Client.Abstractions;

using Cql.Middleware.Library.Shopify;
using Cql.Middleware.Library.Shopify.Order;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using Dasync.Collections;

namespace Cql.Middleware.Impl.Shopify.Services;

public class OrderService : BaseService, IOrderService
{
    public OrderService(ILogger<OrderService> logger, ShopifyGraphQLOptions options)
        : base(logger, options)
    { }

    public async Task<ShopifyOrder> LookupOrder(ShopifyOrderQuery query, CancellationToken cancellationToken = default)
    {
        var orderLookupQuery = @"
query OrdersLookup(
  $orderId: ID!
  $numLineItems: Int!
  $includeTransactions: Boolean!
  $includeShippingLine: Boolean!
  $includeShippingAddress: Boolean!
  $includeBillingAddress: Boolean!
  $includeLineItems: Boolean!
  $includeLineItemVariant: Boolean!
  $includeLineItemProduct: Boolean!
  $lineItemVariantMetafieldsKey: String!
  $metafieldNamespace: String!
  $includefulfillmentOrders: Boolean!
  $includefulfillments: Boolean!
  $includeMetafields: Boolean!
) {
  order(id: $orderId) {
    ...orderFragment
  }
}

fragment orderFragment on Order {
  id
  billingAddress @include(if: $includeBillingAddress) {
    ...mailingAddressFragment
  }
  cancelReason
  cancelledAt
  capturable
  clientIp
  closed
  closedAt
  confirmed
  createdAt
  currencyCode
  customAttributes {
    key
    value
  }
  customerAcceptsMarketing
  customerLocale
  discountCode
  displayFinancialStatus
  displayFulfillmentStatus
  edited
  email
  estimatedTaxes
  fulfillable
  fulfillments @include(if: $includefulfillments) {
    ...fulfillmentFragment
  }
  fulfillmentOrders(first: 2) @include(if: $includefulfillmentOrders) {
    pageInfo {
      ...pageInfoFragment
    }
    edges {
      node {
        ...fulfillmentOrderFragment
      }
    }
  }
  fullyPaid
  hasTimelineComment
  legacyResourceId
  lineItems(first: $numLineItems) @include(if: $includeLineItems) {
    pageInfo {
      ...pageInfoFragment
    }
    edges {
      node {
        ...lineItemFragment
      }
    }
  }
  metafields(first: $numLineItems) @include(if: $includeMetafields) {
    pageInfo {
      ...pageInfoFragment
    }
    edges {
      node {
        ...metafieldFragment
      }
    }
  }
  name
  note
  phone
  presentmentCurrencyCode
  processedAt
  requiresShipping
  riskLevel
  shippingAddress @include(if: $includeShippingAddress) {
    ...mailingAddressFragment
  }
  shippingLine @include(if: $includeShippingLine) {
    ...shippingLineFragment
  }
  sourceIdentifier
  subtotalLineItemsQuantity
  subtotalPriceSet {
    ...moneyBagFragment
  }
  tags
  taxesIncluded
  test
  totalDiscountsSet {
    ...moneyBagFragment
  }
  totalPriceSet {
    ...moneyBagFragment
  }
  totalShippingPriceSet {
    ...moneyBagFragment
  }
  totalTaxSet {
    ...moneyBagFragment
  }
  totalWeight
  transactions @include(if: $includeTransactions) {
    ...transactionFragment
  }
  unpaid
  updatedAt
}

fragment lineItemFragment on LineItem {
  id
  currentQuantity
  customAttributes {
    key
    value
  }
  discountAllocations {
    allocatedAmountSet {
      ...moneyBagFragment
    }
    discountApplication {
      allocationMethod
      index
      targetSelection
      targetType
    }
  }
  discountedTotalSet {
    ...moneyBagFragment
  }
  discountedUnitPriceSet {
    ...moneyBagFragment
  }
  #image
  name
  nonFulfillableQuantity
  #originalTotalSet
  originalUnitPriceSet {
    ...moneyBagFragment
  }
  product @include(if: $includeLineItemProduct) {
    id
  }
  quantity
  refundableQuantity
  requiresShipping
  restockable
  sku
  taxLines {
    ...taxLineFragment
  }
  taxable
  title
  totalDiscountSet {
    ...moneyBagFragment
  }
  unfulfilledQuantity
  variant @include(if: $includeLineItemVariant) {
    ...productVariantFragment
  }
  variantTitle
  vendor
}

fragment taxLineFragment on TaxLine {
  channelLiable
  priceSet {
    ...moneyBagFragment
  }
  rate
  ratePercentage
  title
}

fragment fulfillmentFragment on Fulfillment {
  id
  trackingInfo {
    company
    number
    url
  }
  createdAt
  updatedAt
}

fragment fulfillmentOrderFragment on FulfillmentOrder {
  id
  deliveryMethod {
    id
    methodType
  }
  status
  lineItems(first: $numLineItems) {
    pageInfo {
      ...pageInfoFragment
    }
    edges {
      node {
        ...fulfillmentLineItemFragment
      }
    }
  }
}

fragment fulfillmentLineItemFragment on FulfillmentOrderLineItem {
  id
  remainingQuantity
  totalQuantity
  lineItem {
    id
    variant {
      ...productVariantFragment
    }
  }
}

fragment productVariantFragment on ProductVariant {
  id
  sku
  selectedOptions {
    name
    value
  }
  metafield(key:$lineItemVariantMetafieldsKey, namespace: $metafieldNamespace) {
    ...metafieldFragment
  }
}

fragment shippingLineFragment on ShippingLine {
  id
  carrierIdentifier
  code
  custom
  deliveryCategory
  discountedPriceSet {
      ...moneyBagFragment
  }
  phone
  shippingRateHandle
  source
  taxLines {
      ...taxLineFragment
  }
  title
}

fragment mailingAddressFragment on MailingAddress {
  firstName
  lastName
  address1
  address2
  phone
  zip
  city
  province
  provinceCode
  countryCodeV2
}

fragment transactionFragment on OrderTransaction {
    id
    kind
    gateway
    amountSet {
      ...moneyBagFragment
    }
    accountNumber
    authorizationCode
    authorizationExpiresAt
    createdAt
    errorCode
    formattedGateway
    manuallyCapturable
    processedAt
    receiptJson
    settlementCurrency
    status
    test
    totalUnsettledSet {
      ...moneyBagFragment
    }
}

fragment metafieldFragment on Metafield {
  id
  key
  value
  namespace
}

fragment moneyBagFragment on MoneyBag {
  presentmentMoney {
    ...moneyFragment
  }
  shopMoney {
    ...moneyFragment
  }
}

fragment moneyFragment on MoneyV2 {
  amount
  currencyCode
}

fragment pageInfoFragment on PageInfo {
  hasNextPage
  endCursor
}";

        var result = await ExecuteGraphQLQuery<ShopifyGraphQLOrderResultData>(orderLookupQuery, new
        {
            orderId = query.OrderId,
            numLineItems = query.NumLineItems,
            includeTransactions = query.IncludeTransactions,
            includeShippingLine = query.IncludeShippingLine,
            includeShippingAddress = query.IncludeShippingAddress,
            includeBillingAddress = query.IncludeBillingAddress,
            includeLineItems = query.IncludeLineItems,
            includeLineItemVariant = query.IncludeLineItemVariant,
            includeLineItemProduct = query.IncludeLineItemProduct,
            lineItemVariantMetafieldsKey = query.LineItemVariantMetafieldsKey,
            metafieldNamespace = query.MetafieldNamespace,
            includefulfillmentOrders = query.IncludeFulfillmentOrders,
            includefulfillments = query.IncludeFulfillments,
            includeMetafields = query.IncludeMetafields,
        }, cancellationToken: cancellationToken);
        var order = await AssembleShopifyOrder(query, result.order, cancellationToken);

        return order;
    }

    public async Task<IEnumerable<ShopifyOrder>> LookupOrders(ShopifyOrderQuery query, CancellationToken cancellationToken = default)
    {
        var ordersLookupQuery = @"
query OrdersLookup(
  $ordersQuery: String!
  $numOrders: Int!
  $numLineItems: Int!
  $includeTransactions: Boolean!
  $includeShippingLine: Boolean!
  $includeShippingAddress: Boolean!
  $includeBillingAddress: Boolean!
  $includeLineItems: Boolean!
  $includeLineItemVariant: Boolean!
  $includeLineItemProduct: Boolean!
  $lineItemVariantMetafieldsKey: String!
  $metafieldNamespace: String!
  $includefulfillmentOrders: Boolean!
  $includefulfillments: Boolean!
  $includeMetafields: Boolean!
  $after: String
) {
  orders(
    first: $numOrders,
    query: $ordersQuery,
    after: $after
  ) {
    pageInfo {
      ...pageInfoFragment
    }
    edges {
      node {
        ...orderFragment
      }
    }
  }
}

fragment orderFragment on Order {
  id
  billingAddress @include(if: $includeBillingAddress) {
    ...mailingAddressFragment
  }
  cancelReason
  cancelledAt
  capturable
  clientIp
  closed
  closedAt
  confirmed
  createdAt
  currencyCode
  customAttributes {
    key
    value
  }
  customerAcceptsMarketing
  customerLocale
  discountCode
  displayFinancialStatus
  displayFulfillmentStatus
  edited
  email
  estimatedTaxes
  fulfillable
  fulfillments @include(if: $includefulfillments) {
    ...fulfillmentFragment
  }
  fulfillmentOrders(first: 2) @include(if: $includefulfillmentOrders) {
    pageInfo {
      ...pageInfoFragment
    }
    edges {
      node {
        ...fulfillmentOrderFragment
      }
    }
  }
  fullyPaid
  hasTimelineComment
  legacyResourceId
  lineItems(first: $numLineItems) @include(if: $includeLineItems) {
    pageInfo {
      ...pageInfoFragment
    }
    edges {
      node {
        ...lineItemFragment
      }
    }
  }
  metafields(first: $numLineItems) @include(if: $includeMetafields) {
    pageInfo {
      ...pageInfoFragment
    }
    edges {
      node {
        ...metafieldFragment
      }
    }
  }
  name
  note
  phone
  presentmentCurrencyCode
  processedAt
  requiresShipping
  riskLevel
  shippingAddress @include(if: $includeShippingAddress) {
    ...mailingAddressFragment
  }
  shippingLine @include(if: $includeShippingLine) {
    ...shippingLineFragment
  }
  sourceIdentifier
  subtotalLineItemsQuantity
  subtotalPriceSet {
    ...moneyBagFragment
  }
  tags
  taxesIncluded
  test
  totalDiscountsSet {
    ...moneyBagFragment
  }
  totalPriceSet {
    ...moneyBagFragment
  }
  totalShippingPriceSet {
    ...moneyBagFragment
  }
  totalTaxSet {
    ...moneyBagFragment
  }
  totalWeight
  transactions @include(if: $includeTransactions) {
    ...transactionFragment
  }
  unpaid
  updatedAt
}

fragment lineItemFragment on LineItem {
  id
  currentQuantity
  customAttributes {
    key
    value
  }
  discountAllocations {
    allocatedAmountSet {
      ...moneyBagFragment
    }
    discountApplication {
      allocationMethod
      index
      targetSelection
      targetType
    }
  }
  discountedTotalSet {
    ...moneyBagFragment
  }
  discountedUnitPriceSet {
    ...moneyBagFragment
  }
  #image
  name
  nonFulfillableQuantity
  #originalTotalSet
  originalUnitPriceSet {
    ...moneyBagFragment
  }
  product @include(if: $includeLineItemProduct) {
    id
  }
  quantity
  refundableQuantity
  requiresShipping
  restockable
  sku
  taxLines {
    ...taxLineFragment
  }
  taxable
  title
  totalDiscountSet {
    ...moneyBagFragment
  }
  unfulfilledQuantity
  variant @include(if: $includeLineItemVariant) {
    ...productVariantFragment
  }
  variantTitle
  vendor
}

fragment taxLineFragment on TaxLine {
  channelLiable
  priceSet {
    ...moneyBagFragment
  }
  rate
  ratePercentage
  title
}


fragment fulfillmentFragment on Fulfillment {
  id
  trackingInfo {
    company
    number
    url
  }
  createdAt
  updatedAt
}

fragment fulfillmentOrderFragment on FulfillmentOrder {
  id
  deliveryMethod {
    id
    methodType
  }
  status
  lineItems(first: $numLineItems) {
    pageInfo {
      ...pageInfoFragment
    }
    edges {
      node {
        ...fulfillmentLineItemFragment
      }
    }
  }
}

fragment fulfillmentLineItemFragment on FulfillmentOrderLineItem {
  id
  remainingQuantity
  totalQuantity
  lineItem {
    id
    variant {
      ...productVariantFragment
    }
  }
}

fragment productVariantFragment on ProductVariant {
  id
  sku
  selectedOptions {
    name
    value
  }
  metafield(key:$lineItemVariantMetafieldsKey, namespace: $metafieldNamespace) {
    ...metafieldFragment
  }
}

fragment shippingLineFragment on ShippingLine {
  id
  carrierIdentifier
  code
  custom
  deliveryCategory
  discountedPriceSet {
      ...moneyBagFragment
  }
  phone
  shippingRateHandle
  source
  taxLines {
      ...taxLineFragment
  }
  title
}

fragment mailingAddressFragment on MailingAddress {
  firstName
  lastName
  address1
  address2
  phone
  zip
  city
  province
  provinceCode
  countryCodeV2
}

fragment transactionFragment on OrderTransaction {
    id
    kind
    gateway
    amountSet {
      ...moneyBagFragment
    }
    accountNumber
    authorizationCode
    authorizationExpiresAt
    createdAt
    errorCode
    formattedGateway
    manuallyCapturable
    processedAt
    receiptJson
    settlementCurrency
    status
    test
    totalUnsettledSet {
      ...moneyBagFragment
    }
}

fragment metafieldFragment on Metafield {
  id
  key
  value
  namespace
}

fragment moneyBagFragment on MoneyBag {
  presentmentMoney {
    ...moneyFragment
  }
  shopMoney {
    ...moneyFragment
  }
}

fragment moneyFragment on MoneyV2 {
  amount
  currencyCode
}

fragment pageInfoFragment on PageInfo {
  hasNextPage
  endCursor
}";

        var ordersResult = new ConcurrentBag<ShopifyOrder>();
        var after = default(String?);
        var hasNextPage = false;
        do
        {
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLOrderLookupResultData>(ordersLookupQuery, new
            {
                ordersQuery = query.OrdersQuery,
                numOrders = query.NumOrders,
                numLineItems = query.NumLineItems,
                includeTransactions = query.IncludeTransactions,
                includeShippingLine = query.IncludeShippingLine,
                includeShippingAddress = query.IncludeShippingAddress,
                includeBillingAddress = query.IncludeBillingAddress,
                includeLineItems = query.IncludeLineItems,
                includeLineItemVariant = query.IncludeLineItemVariant,
                includeLineItemProduct = query.IncludeLineItemProduct,
                lineItemVariantMetafieldsKey = query.LineItemVariantMetafieldsKey,
                metafieldNamespace = query.MetafieldNamespace,
                includefulfillmentOrders = query.IncludeFulfillmentOrders,
                includefulfillments = query.IncludeFulfillments,
                includeMetafields = query.IncludeMetafields,
                after = after
            }, cancellationToken: cancellationToken);
            var orders = result?.orders ?? new ShopifyGraphQLResultPage<ShopifyGraphQLOrder>();
            hasNextPage = orders.pageInfo.hasNextPage;
            if (hasNextPage)
            {
                after = orders.pageInfo.endCursor;
            }

            await orders.ToList().ParallelForEachAsync(async (node) =>
            {
                ordersResult.Add(await AssembleShopifyOrder(query, node, cancellationToken));
            });

        } while (hasNextPage);

        return ordersResult.ToList();
    }

    public async Task<ShopifyOrder> UpdateOrderTags(string id, IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        var updateOrderQuery = @"
mutation OrderUpdateTags($input: OrderInput!) {
  orderUpdate(input: $input) {
    order {
      ...orderFragment
    }
    userErrors {
      ...userErrorParts
    }
  }
}

fragment orderFragment on Order {
  id
  tags
}

fragment userErrorParts on UserError {
  field
  message
}";
        var result = await ExecuteGraphQLQuery<ShopifyGraphQLOrderUpdateResultData>(updateOrderQuery, new
        {
            input = new
            {
                id,
                tags
            }
        }, cancellationToken: cancellationToken);

        if (result.orderUpdate.userErrors.Any())
        {
            foreach (var userError in result.orderUpdate.userErrors)
            {
                _logger.LogError("Error on OrderUpdateTags: {0} - {1} - {2}", id, userError.field?.FirstOrDefault(), userError.message);
            }
        }
        return result.orderUpdate.order.Map();
    }

    public async Task<IEnumerable<ShopifyOrder>> LookupFailedOrders(ShopifyOrderQuery query, CancellationToken cancellationToken = default)
    {
        var MissedOrdersQuery = @"
query MissedOrdersLookup(
  $ordersQuery: String!
  $numOrders: Int
  $after: String
) {
  orders(first: $numOrders, query: $ordersQuery, after: $after) {
    pageInfo {
      ...pageInfoFragment
    }
    edges {
      node {
        ...orderFragment
      }
    }
  }
}

fragment orderFragment on Order {
  id
  cancelReason
  cancelledAt
  capturable
  clientIp
  closed
  closedAt
  confirmed
  createdAt
  currencyCode
  customerAcceptsMarketing
  customerLocale
  discountCode
  displayFinancialStatus
  displayFulfillmentStatus
  edited
  email
  estimatedTaxes
  fulfillable
  fullyPaid
  hasTimelineComment
  legacyResourceId
  name
  note
  phone
  presentmentCurrencyCode
  processedAt
  requiresShipping
  riskLevel
  sourceIdentifier
  subtotalLineItemsQuantity
  subtotalPriceSet {
    ...moneyBagFragment
  }
  tags
  taxesIncluded
  test
  totalDiscountsSet {
    ...moneyBagFragment
  }
  totalPriceSet {
    ...moneyBagFragment
  }
  totalShippingPriceSet {
    ...moneyBagFragment
  }
  totalTaxSet {
    ...moneyBagFragment
  }
  totalWeight
  unpaid
  updatedAt
}

fragment moneyBagFragment on MoneyBag {
  presentmentMoney {
    ...moneyFragment
  }
  shopMoney {
    ...moneyFragment
  }
}

fragment moneyFragment on MoneyV2 {
  amount
  currencyCode
}

fragment pageInfoFragment on PageInfo {
  hasNextPage
  endCursor
}";
        var ordersResult = new List<ShopifyOrder>();
        var after = default(String?);
        var hasNextPage = false;
        do
        {
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLOrderLookupResultData>(MissedOrdersQuery, new
            {
                ordersQuery = query.OrdersQuery,
                numOrders = query.NumFailedOrders,
                after = after
            }, cancellationToken: cancellationToken);
            var orders = result?.orders ?? new ShopifyGraphQLResultPage<ShopifyGraphQLOrder>();
            hasNextPage = orders.pageInfo.hasNextPage;
            if (hasNextPage)
            {
                after = orders.pageInfo.endCursor;
            }
            ordersResult.AddRange(orders.ToList().Select((o) => o.Map()));

        } while (hasNextPage);

        return ordersResult.ToList();
    }

    private async Task<IList<ShopifyGraphQLLineItem>> GetAdditionalLineItems(ShopifyOrderQuery query, string orderId, string? cursor = null, CancellationToken cancellationToken = default)
    {
        var graphQLQuery = @"
query OrderItemsLookup(
  $orderId: ID!
  $includeLineItemVariant: Boolean!
  $includeLineItemProduct: Boolean!
  $metafieldNamespace: String!
  $lineItemVariantMetafieldsKey: String!
  $numLineItems: Int
  $after: String
) {
  order(
    id: $orderId
  ) {
    lineItems(first:$numLineItems, after: $after) {
      pageInfo {
        ...pageInfoFragment
      }
      edges {
        node {
          ...lineItemFragment
        }
      }
    }
  }
}

fragment lineItemFragment on LineItem {
  id
  currentQuantity
  customAttributes {
      key
      value
  }
  discountAllocations {
    allocatedAmountSet {
      ...moneyBagFragment
    }
    discountApplication {
      allocationMethod
      index
      targetSelection
      targetType
    }
  }
  discountedTotalSet {
    ...moneyBagFragment
  }
  discountedUnitPriceSet {
    ...moneyBagFragment
  }
  #image
  name
  nonFulfillableQuantity
  #originalTotalSet
  originalUnitPriceSet {
    ...moneyBagFragment
  }
  product @include(if: $includeLineItemProduct) {
    id
  }
  quantity
  refundableQuantity
  requiresShipping
  restockable
  sku
  taxLines {
    ...taxLineFragment
  }
  taxable
  title
  totalDiscountSet {
    ...moneyBagFragment
  }
  unfulfilledQuantity
  variant @include(if: $includeLineItemVariant) {
      ...productVariantFragment
  }
  variantTitle
  vendor
}

fragment taxLineFragment on TaxLine {
  channelLiable
  priceSet {
    ...moneyBagFragment
  }
  rate
  ratePercentage
  title
}

fragment productVariantFragment on ProductVariant {
  id
  sku
  selectedOptions{
    name
    value
  }
  metafield(key:$lineItemVariantMetafieldsKey, namespace: $metafieldNamespace) {
    ...metafieldFragment
  }
}

fragment metafieldFragment on Metafield {
    id
    key
    value
    namespace
}

fragment moneyBagFragment on MoneyBag {
  presentmentMoney {
    ...moneyFragment
  }
  shopMoney {
    ...moneyFragment
  }
}

fragment moneyFragment on MoneyV2 {
  amount
  currencyCode
}

fragment pageInfoFragment on PageInfo {
  hasNextPage
  endCursor
}";

        var rval = new List<ShopifyGraphQLLineItem>();
        var after = cursor;
        var hasNextPage = false;
        do
        {
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLOrderResultData>(graphQLQuery, new
            {
                orderId = orderId,
                includeLineItemVariant = query.IncludeLineItemVariant,
                includeLineItemProduct = query.IncludeLineItemProduct,
                lineItemVariantMetafieldsKey = query.LineItemVariantMetafieldsKey,
                metafieldNamespace = query.MetafieldNamespace,
                numLineItems = query.NumAdditionalLineItems,
                after = after
            }, cancellationToken: cancellationToken);
            var lineItems = result.order.lineItems ?? new ShopifyGraphQLResultPage<ShopifyGraphQLLineItem>();
            hasNextPage = lineItems.pageInfo.hasNextPage;
            if (hasNextPage)
            {
                after = lineItems.pageInfo.endCursor;
            }
            rval.AddRange(lineItems.ToList());

        } while (hasNextPage);

        return rval;
    }

    private async Task<IList<ShopifyGraphQLMetafield>> GetAdditionalMetafields(ShopifyOrderQuery query, string orderId, string? cursor = null, CancellationToken cancellationToken = default)
    {
        var graphQLQuery = @"
query OrderItemsLookup(
  $orderId: ID!
  $metafieldNamespace: String!
  $numMetafields: Int
  $after: String
) {
  order(
    id: $orderId
  ) {
    metafields(first:$numMetafields, namespace:$metafieldNamespace, after: $after) {
      pageInfo {
        ...pageInfoFragment
      }
      edges {
        node {
          ...metafieldFragment
        }
      }
    }
  }
}

fragment metafieldFragment on Metafield {
  id
  key
  value
  namespace
}

fragment pageInfoFragment on PageInfo {
  hasNextPage
  endCursor
}";

        var rval = new List<ShopifyGraphQLMetafield>();
        var after = cursor;
        var hasNextPage = false;
        do
        {
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLOrderResultData>(graphQLQuery, new
            {
                orderId = orderId,
                metafieldNamespace = query.MetafieldNamespace,
                numMetafields = query.NumAdditionalLineItems,
                after = after
            }, cancellationToken: cancellationToken);
            var metafields = result.order.metafields ?? new ShopifyGraphQLResultPage<ShopifyGraphQLMetafield>();
            hasNextPage = metafields.pageInfo.hasNextPage;
            if (hasNextPage)
            {
                after = metafields.pageInfo.endCursor;
            }
            rval.AddRange(metafields.ToList());

        } while (hasNextPage);

        return rval;
    }

    private async Task<IList<ShopifyGraphQLFulfillmentOrderLineItem>> GetAdditionalFulfillmentOrderLineItems(ShopifyOrderQuery query, string orderId, string? cursor = null, CancellationToken cancellationToken = default)
    {
        var graphQLQuery = @"
query FulfillmentOrderItemsLookup(
  $orderId: ID!
  $numLineItems: Int!
  $metafieldNamespace: String!
  $lineItemVariantMetafieldsKey: String!
  $after: String!
) {
  order(id: $orderId) {
    fulfillmentOrders(first: 2) {
      edges {
        node {
          id
          status
          lineItems(first: $numLineItems, after: $after) {
            pageInfo {
              ...pageInfoFragment
            }
            edges {
              node {
                ...fulfillmentLineItemFragment
              }
            }
          }
        }
      }
    }
  }
}
fragment fulfillmentLineItemFragment on FulfillmentOrderLineItem {
  id
  remainingQuantity
  totalQuantity
  lineItem {
    id
    variant {
      ...productVariantFragment
    }
  }
}

fragment productVariantFragment on ProductVariant {
  id
  sku
  selectedOptions {
    name
    value
  }
  metafield(key:$lineItemVariantMetafieldsKey, namespace: $metafieldNamespace) {
    ...metafieldFragment
  }
}

fragment metafieldFragment on Metafield {
  id
  key
  value
  namespace
}

fragment pageInfoFragment on PageInfo {
  hasNextPage
  endCursor
}
";

        var rval = new List<ShopifyGraphQLFulfillmentOrderLineItem>();
        var after = cursor;
        var hasNextPage = false;
        do
        {
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLOrderResultData>(graphQLQuery, new
            {
                orderId = orderId,
                numLineItems = query.NumAdditionalLineItems,
                lineItemVariantMetafieldsKey = query.LineItemVariantMetafieldsKey,
                metafieldNamespace = query.MetafieldNamespace,
                after = after
            }, cancellationToken: cancellationToken);
            var fulfillmentOrder = result.order.fulfillmentOrders?.edges?.FirstOrDefault()?.node ?? new ShopifyGraphQLFulfillmentOrder();

            var lineItems = fulfillmentOrder.lineItems ?? new ShopifyGraphQLResultPage<ShopifyGraphQLFulfillmentOrderLineItem>();
            hasNextPage = lineItems.pageInfo.hasNextPage;
            if (hasNextPage)
            {
                after = lineItems.pageInfo.endCursor;
            }
            rval.AddRange(lineItems.ToList());

        } while (hasNextPage);

        return rval;
    }

    private async Task<ShopifyOrder> AssembleShopifyOrder(ShopifyOrderQuery query, ShopifyGraphQLOrder node, CancellationToken cancellationToken = default)
    {
        if (!String.IsNullOrEmpty(node.id) && node.lineItems != null && node.lineItems.pageInfo.hasNextPage)
        {
            var additionalLineItems = await GetAdditionalLineItems(query, node.id, node.lineItems.pageInfo.endCursor, cancellationToken);
            if (additionalLineItems?.Any() ?? false)
            {
                var items = node.lineItems.ToList();
                items.AddRange(additionalLineItems);
                node.lineItems = new ShopifyGraphQLResultPage<ShopifyGraphQLLineItem>
                {
                    pageInfo = new ShopifyGraphQLPageInfo(),
                    edges = items.Select(i => new ShopifyGraphQLResultEdge<ShopifyGraphQLLineItem> { node = i }).ToArray()
                };
            }
        }

        if (!String.IsNullOrEmpty(node.id))
        {
            foreach (var edges in node.fulfillmentOrders?.edges ?? Array.Empty<ShopifyGraphQLResultEdge<ShopifyGraphQLFulfillmentOrder>>())
            {
                var fulfillmentOrder = edges.node;
                if (fulfillmentOrder != null && !String.IsNullOrEmpty(fulfillmentOrder.id) && fulfillmentOrder.lineItems != null && fulfillmentOrder.lineItems.pageInfo.hasNextPage)
                {
                    var additionalLineItems = await GetAdditionalLineItemsForFulfillmentOrder(query, node.id, fulfillmentOrder, cancellationToken);
                    if (additionalLineItems?.Any() ?? false)
                    {
                        var items = fulfillmentOrder.lineItems.ToList();
                        items.AddRange(additionalLineItems);
                        fulfillmentOrder.lineItems = new ShopifyGraphQLResultPage<ShopifyGraphQLFulfillmentOrderLineItem>
                        {
                            pageInfo = new ShopifyGraphQLPageInfo(),
                            edges = items.Select(i => new ShopifyGraphQLResultEdge<ShopifyGraphQLFulfillmentOrderLineItem> { node = i }).ToArray()
                        };
                    }
                }
            }
        }

        if (!String.IsNullOrEmpty(node.id) && node.metafields != null && node.metafields.pageInfo.hasNextPage)
        {
            var additionalMetafields = await GetAdditionalMetafields(query, node.id, node.metafields.pageInfo.endCursor, cancellationToken);
            if (additionalMetafields?.Any() ?? false)
            {
                var metafields = node.metafields.ToList();
                metafields.AddRange(additionalMetafields);
                node.metafields = new ShopifyGraphQLResultPage<ShopifyGraphQLMetafield>
                {
                    pageInfo = new ShopifyGraphQLPageInfo(),
                    edges = metafields.Select(i => new ShopifyGraphQLResultEdge<ShopifyGraphQLMetafield> { node = i }).ToArray()
                };
            }
        }

        return node.Map();
    }

    private async Task<IList<ShopifyGraphQLFulfillmentOrderLineItem>?> GetAdditionalLineItemsForFulfillmentOrder(ShopifyOrderQuery query, string orderId, ShopifyGraphQLFulfillmentOrder fulfillmentOrder, CancellationToken cancellationToken = default)
    {
        IList<ShopifyGraphQLFulfillmentOrderLineItem>? additionalFulfillmentOrderLineItems = null;

        if (fulfillmentOrder != null && !String.IsNullOrEmpty(orderId) && fulfillmentOrder.lineItems != null && fulfillmentOrder.lineItems.pageInfo.hasNextPage)
        {
            additionalFulfillmentOrderLineItems = await GetAdditionalFulfillmentOrderLineItems(query, orderId, fulfillmentOrder.lineItems.pageInfo.endCursor, cancellationToken);
        }
        return additionalFulfillmentOrderLineItems;
    }
}

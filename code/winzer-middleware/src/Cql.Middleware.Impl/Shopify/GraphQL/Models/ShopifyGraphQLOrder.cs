namespace CQL.Middleware.Impl.Shopify.GraphQL.Models;

public class ShopifyGraphQLOrderLookupResultData
{
    public ShopifyGraphQLResultPage<ShopifyGraphQLOrder>? orders { get; set; }
}

public class ShopifyGraphQLOrderResultData
{
    public ShopifyGraphQLOrder order { get; set; } = new ShopifyGraphQLOrder();
}

public class ShopifyGraphQLOrderUpdateResultData
{
    public ShopifyGraphQLOrderUpdateResult orderUpdate { get; set; } = new ShopifyGraphQLOrderUpdateResult();
}

public class ShopifyGraphQLOrderUpdateResult
{
    public ShopifyGraphQLOrder order { get; set; } = new ShopifyGraphQLOrder();
    public IEnumerable<ShopifyGraphQLUserError> userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
}

public class ShopifyGraphQLOrder
{
    public String? id { get; set; }
    public ShopifyGraphQLMailingAddress? billingAddress { get; set; }
    public String? cancelReason { get; set; }
    public DateTime? cancelledAt { get; set; }
    public Boolean? capturable { get; set; }
    public ShopifyGraphQLMoneyBag? cartDiscountAmountSet { get; set; }
    public String? clientIp { get; set; }
    public Boolean? closed { get; set; }
    public DateTime? closedAt { get; set; }
    public Boolean? confirmed { get; set; }
    public DateTime? createdAt { get; set; }
    public String? currencyCode { get; set; }
    public ShopifyGraphQLMoneyBag? currentCartDiscountAmountSet { get; set; }
    public int? currentSubtotalLineItemsQuantity { get; set; }
    public ShopifyGraphQLMoneyBag? currentSubtotalPriceSet { get; set; }
    public IEnumerable<ShopifyGraphQLTaxLine>? currentTaxLines { get; set; }
    public ShopifyGraphQLMoneyBag? currentTotalDiscountsSet { get; set; }
    public ShopifyGraphQLMoneyBag? currentTotalDutiesSet { get; set; }
    public ShopifyGraphQLMoneyBag? currentTotalPriceSet { get; set; }
    public ShopifyGraphQLMoneyBag? currentTotalTaxSet { get; set; }
    public UInt64? currentTotalWeight { get; set; }
    public IEnumerable<ShopifyGraphQLAttribute>? customAttributes { get; set; }
    public ShopifyGraphQLCustomer? customer { get; set; }
    public Boolean? customerAcceptsMarketing { get; set; }
    public String? customerLocale { get; set; }
    public IEnumerable<ShopifyGraphQLDiscountApplication>? discountApplications { get; set; }
    public String? discountCode { get; set; }
    public ShopifyGraphQLMailingAddress? displayAddress { get; set; }
    public String? displayFinancialStatus { get; set; }
    public String? displayFulfillmentStatus { get; set; }
    public Boolean? edited { get; set; }
    public String? email { get; set; }
    public Boolean? estimatedTaxes { get; set; }
    public Boolean? fulfillable { get; set; }
    public ShopifyGraphQLResultPage<ShopifyGraphQLFulfillmentOrder>? fulfillmentOrders { get; set; }
    public IEnumerable<ShopifyGraphQLFulfillment> fulfillments { get; set; } = Enumerable.Empty<ShopifyGraphQLFulfillment>();
    public Boolean? fullyPaid { get; set; }
    public Boolean? hasTimelineComment { get; set; }
    public UInt64? legacyResourceId { get; set; }
    public ShopifyGraphQLResultPage<ShopifyGraphQLLineItem>? lineItems { get; set; }
    public ShopifyGraphQLMetafield? metafield { get; set; }
    public ShopifyGraphQLResultPage<ShopifyGraphQLMetafield>? metafields { get; set; }
    public String? name { get; set; }
    public ShopifyGraphQLMoneyBag? netPaymentSet { get; set; }
    public String? note { get; set; }
    public ShopifyGraphQLMoneyBag? originalTotalDutiesSet { get; set; }
    public ShopifyGraphQLMoneyBag? originalTotalPriceSet { get; set; }
    public String? phone { get; set; }
    public ShopifyGraphQLLocation? physicalLocation { get; set; }
    public String? presentmentCurrencyCode { get; set; }
    public DateTime? processedAt { get; set; }
    public Boolean? requiresShipping { get; set; }
    public String? riskLevel { get; set; }
    public ShopifyGraphQLMailingAddress? shippingAddress { get; set; }
    public ShopifyGraphQLShippingLine? shippingLine { get; set; }
    public IEnumerable<ShopifyGraphQLShippingLine>? shippingLines { get; set; }
    public String? sourceIdentifier { get; set; }
    public int? subtotalLineItemsQuantity { get; set; }
    public ShopifyGraphQLMoneyBag? subtotalPriceSet { get; set; }
    public IEnumerable<String>? tags { get; set; }
    public IEnumerable<ShopifyGraphQLTaxLine>? taxLines { get; set; }
    public Boolean? taxesIncluded { get; set; }
    public Boolean? test { get; set; }
    public ShopifyGraphQLMoneyBag? totalCapturableSet { get; set; }
    public ShopifyGraphQLMoneyBag? totalDiscountsSet { get; set; }
    public ShopifyGraphQLMoneyBag? totalOutstandingSet { get; set; }
    public ShopifyGraphQLMoneyBag? totalPriceSet { get; set; }
    public ShopifyGraphQLMoneyBag? totalReceivedSet { get; set; }
    public ShopifyGraphQLMoneyBag? totalRefundedSet { get; set; }
    public ShopifyGraphQLMoneyBag? totalRefundedShippingSet { get; set; }
    public ShopifyGraphQLMoneyBag? totalShippingPriceSet { get; set; }
    public ShopifyGraphQLMoneyBag? totalTaxSet { get; set; }
    public ShopifyGraphQLMoneyBag? totalTipReceivedSet { get; set; }
    public UInt64? totalWeight { get; set; }
    public IEnumerable<ShopifyGraphQLOrderTransaction>? transactions { get; set; }
    public Boolean? unpaid { get; set; }
    public DateTime? updatedAt { get; set; }
}

public class ShopifyGraphQLOrderTransaction
{
    public String? id { get; set; }
    public String? kind { get; set; }
    public String? gateway { get; set; }
    public ShopifyGraphQLMoneyBag? amountSet { get; set; }
    public String? accountNumber { get; set; }
    public String? authorizationCode { get; set; }
    public DateTime? authorizationExpiresAt { get; set; }
    public DateTime? createdAt { get; set; }
    public String? errorCode { get; set; }
    public IEnumerable<ShopifyGraphQLTransactionFee>? fees { get; set; }
    public String? formattedGateway { get; set; }
    public Boolean? manuallyCapturable { get; set; }
    public ShopifyGraphQLMoney? maximumRefundableV2 { get; set; }
    public ShopifyGraphQLOrder? order { get; set; }
    public ShopifyGraphQLOrderTransaction? parentTransaction { get; set; }
    public ShopifyGraphQLImage? paymentIcon { get; set; }
    public DateTime? processedAt { get; set; }
    public String? receiptJson { get; set; }
    public String? settlementCurrency { get; set; }
    public Decimal? settlementCurrencyRate { get; set; }
    public String? status { get; set; }
    public Boolean? test { get; set; }
    public ShopifyGraphQLMoneyBag? totalUnsettledSet { get; set; }
}

public class ShopifyGraphQLTransactionFee
{
    public String? id { get; set; }
    public ShopifyGraphQLMoney? amount { get; set; }
    public ShopifyGraphQLMoney? flatFee { get; set; }
    public String? flatFeeName { get; set; }
    public Decimal? rate { get; set; }
    public String? rateName { get; set; }
    public ShopifyGraphQLMoney? taxAmount { get; set; }
    public String? type { get; set; }
}

public class ShopifyGraphQLTaxLine
{
    public Boolean? channelLiable  {get; set; }
    public ShopifyGraphQLMoneyBag? priceSet { get; set; }
    public decimal? rate { get; set; }
    public decimal? ratePercentage { get; set; }
    public String? title { get; set; }
}

public class ShopifyGraphQLShippingLine
{
    public String? id { get; set; }
    public String? carrierIdentifier { get; set; }
    public String? code { get; set; }
    public Boolean custom { get; set; }
    public String? deliveryCategory { get; set; }
    public IEnumerable<ShopifyGraphQLDiscountAllocation>? discountAllocations { get; set; }
    public ShopifyGraphQLMoneyBag? discountedPriceSet { get; set; }
    public ShopifyGraphQLMoneyBag? originalPriceSet { get; set; }
    public String? phone { get; set; }
    public String? shippingRateHandle { get; set; }
    public String? source { get; set; }
    public IEnumerable<ShopifyGraphQLTaxLine>? taxLines { get; set; }
    public String? title { get; set; }
}

public class ShopifyGraphQLLineItem
{
    public int? currentQuantity { get; set; }
    public IEnumerable<ShopifyGraphQLAttribute>? customAttributes { get; set; }
    public IEnumerable<ShopifyGraphQLDiscountAllocation>? discountAllocations { get; set; }
    public ShopifyGraphQLMoneyBag? discountedTotalSet { get; set; }
    public ShopifyGraphQLMoneyBag? discountedUnitPriceSet { get; set; }
    public String? id { get; set; }
    public ShopifyGraphQLImage? image { get; set; }
    public Boolean? merchantEditable { get; set; }
    public String? name { get; set; }
    public int? nonFulfillableQuantity { get; set; }
    public ShopifyGraphQLMoneyBag? originalTotalSet { get; set; }
    public ShopifyGraphQLMoneyBag? originalUnitPriceSet { get; set; }
    public ShopifyGraphQLProduct? product { get; set; }
    public int? quantity { get; set; }
    public int? refundableQuantity { get; set; }
    public Boolean? requiresShipping { get; set; }
    public Boolean? restockable { get; set; }
    public String? sku { get; set; }
    public IEnumerable<ShopifyGraphQLTaxLine>? taxLines { get; set; }
    public Boolean? taxable { get; set; }
    public String? title { get; set; }
    public ShopifyGraphQLMoneyBag? totalDiscountSet { get; set; }
    public ShopifyGraphQLMoneyBag? unfulfilledDiscountedTotalSet { get; set; }
    public ShopifyGraphQLMoneyBag? unfulfilledOriginalTotalSet { get; set; }
    public int? unfulfilledQuantity { get; set; }
    public ShopifyGraphQLProductVariant? variant { get; set; }
    public String? variantTitle { get; set; }
    public String? vendor { get; set; }
}

public class ShopifyGraphQLDiscountAllocation
{
    public ShopifyGraphQLMoneyBag? allocatedAmountSet { get; set; }
    public ShopifyGraphQLDiscountApplication? discountApplication { get; set; }
}

public class ShopifyGraphQLDiscountApplication
{
    public String? allocationMethod { get; set; }
    public int? index { get; set; }
    public String? targetSelection { get; set; }
    public String? targetType { get; set; }
    public ShopifyGraphQLPricingValue? value { get; set; }
}

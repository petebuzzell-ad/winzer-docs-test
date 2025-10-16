using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Customer;
using Cql.Middleware.Library.Shopify.Store;
using Cql.Middleware.Library.Shopify.Shipping;
using Cql.Middleware.Library.Shopify.Products;

namespace Cql.Middleware.Library.Shopify.Order;

public class ShopifyOrder
{
    /// <summary>
    /// A globally-unique identifier.
    /// </summary>
    public String Id { get; set; } = String.Empty;

    /// <summary>
    /// The billing address of the customer.
    /// </summary>
    public ShopifyMailingAddress? BillingAddress { get; set; }

    /// <summary>
    /// The reason provided when the order was canceled.Returns `null` if the order wasn't canceled.
    /// </summary>
    public String? CancelReason { get; set; }

    /// <summary>
    /// The date and time when the order was canceled.Returns `null` if the order wasn't canceled.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// Whether payment for the order can be captured.
    /// </summary>
    public Boolean Capturable { get; set; }

    /// <summary>
    /// The total order-level discount amount, before returns, in shop and presentment currencies.
    /// </summary>
    public MoneyBag? CartDiscountAmountSet { get; set; }

    /// <summary>
    /// The IP address of the API client that created the order.
    /// </summary>
    public String? ClientIp { get; set; }

    /// <summary>
    /// Whether the order is closed.
    /// </summary>
    public Boolean Closed { get; set; }

    /// <summary>
    /// The date and time when the order was closed.Returns `null` if the order is not closed.
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// Whether inventory has been reserved for the order.
    /// </summary>
    public Boolean Confirmed { get; set; }

    /// <summary>
    /// Date and time when the order was created in Shopify.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The shop currency when the order was placed.
    /// </summary>
    public String CurrencyCode { get; set; } = String.Empty;

    /// <summary>
    /// The current order-level discount amount after all order updates, in shop and presentment currencies.
    /// </summary>
    public MoneyBag CurrentCartDiscountAmountSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The sum of the quantities for all line items that contribute to the order's current subtotal price.
    /// </summary>
    public int CurrentSubtotalLineItemsQuantity { get; set; }

    /// <summary>
    /// The sum of the prices for all line items after discounts and returns, in shop and presentment currencies.If `taxesIncluded` is `true`, then the subtotal also includes tax.
    /// </summary>
    public MoneyBag CurrentSubtotalPriceSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// A list of all tax lines applied to line items on the order, after returns.Tax line prices represent the total price for all tax lines with the same `rate` and `title`.
    /// </summary>
    public IEnumerable<ShopifyTaxLine> CurrentTaxLines { get; set; } = Enumerable.Empty<ShopifyTaxLine>();

    /// <summary>
    /// The total amount discounted on the order after returns, in shop and presentment currencies.This includes both order and line level discounts.
    /// </summary>
    public MoneyBag CurrentTotalDiscountsSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The total amount of duties after returns, in shop and presentment currencies.Returns `null` if duties aren't applicable.
    /// </summary>
    public MoneyBag? CurrentTotalDutiesSet { get; set; }

    /// <summary>
    /// The total price of the order, after returns, in shop and presentment currencies.This includes taxes and discounts.
    /// </summary>
    public MoneyBag CurrentTotalPriceSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The sum of the prices of all tax lines applied to line items on the order, after returns, in shop and presentment currencies.
    /// </summary>
    public MoneyBag CurrentTotalTaxSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The total weight of the order after returns, in grams.
    /// </summary>
    public UInt64 CurrentTotalWeight { get; set; }

    /// <summary>
    /// A list of the custom attributes added to the order.
    /// </summary>
    public IEnumerable<ShopifyAttribute> CustomAttributes { get; set; } = Enumerable.Empty<ShopifyAttribute>();

    /// <summary>
    /// The customer that placed the order.
    /// </summary>
    public ShopifyCustomer? Customer { get; set; }

    /// <summary>
    /// Whether the customer agreed to receive marketing materials.
    /// </summary>
    public Boolean CustomerAcceptsMarketing { get; set; }

    /// <summary>
    /// A two-letter or three-letter language code, optionally followed by a region modifier.
    /// </summary>
    public String? CustomerLocale { get; set; }

    /// <summary>
    /// A list of discounts that are applied to the order.
    /// </summary>
    public IEnumerable<ShopifyDiscountApplication> DiscountApplications { get; set; } = Enumerable.Empty<ShopifyDiscountApplication>();

    /// <summary>
    /// The discount code used for the order.
    /// </summary>
    public String? DiscountCode { get; set; }

    /// <summary>
    /// The primary address of the customer.Returns `null` if neither the shipping address nor the billing address was provided.
    /// </summary>
    public ShopifyMailingAddress? DisplayAddress { get; set; }

    /// <summary>
    /// The financial status of the order that can be shown to the merchant.This field does not capture all the details of an order's financial state. It should only be used for display summary purposes.
    /// </summary>
    public String? DisplayFinancialStatus { get; set; }

    /// <summary>
    /// The fulfillment status for the order that can be shown to the merchant.This field does not capture all the details of an order's fulfillment state. It should only be used for display summary purposes.For a more granular view of the fulfillment status, refer to the [FulfillmentOrder](https:\/\/shopify.dev\/api\/admin-graphql\/latest\/objects\/FulfillmentOrder) object.
    /// </summary>
    public String? DisplayFulfillmentStatus { get; set; }

    /// <summary>
    /// Whether the order has had any edits applied.
    /// </summary>
    public Boolean Edited { get; set; }

    /// <summary>
    /// The email address associated with the customer.
    /// </summary>
    public String? Email { get; set; }

    /// <summary>
    /// Whether taxes on the order are estimated.This field returns `false` when taxes on the order are finalized and aren't subject to any changes.
    /// </summary>
    public Boolean EstimatedTaxes { get; set; }

    /// <summary>
    /// Whether there are line items that can be fulfilled.This field returns `false` when the order has no fulfillable line items.For a more granular view of the fulfillment status, refer to the [FulfillmentOrder](https:\/\/shopify.dev\/api\/admin-graphql\/latest\/objects\/FulfillmentOrder) object.
    /// </summary>
    public Boolean Fulfillable { get; set; }

    /// <summary>
    /// A list of fulfillment orders for the order.
    /// </summary>
    public IEnumerable<FulfillmentOrder> FulfillmentOrders { get; set; } = Enumerable.Empty<FulfillmentOrder>();

    /// <summary>
    /// List of shipments for the order.
    /// </summary>
    public IEnumerable<ShopifyFulfillment> Fulfillments { get; set; } = Enumerable.Empty<ShopifyFulfillment>();

    /// <summary>
    /// Whether the order has been paid in full.
    /// </summary>
    public Boolean FullyPaid { get; set; }

    /// <summary>
    /// Whether the merchant added a timeline comment to the order.
    /// </summary>
    public Boolean HasTimelineComment { get; set; }

    /// <summary>
    /// The ID of the corresponding resource in the REST Admin API.
    /// </summary>
    public UInt64 LegacyResourceId { get; set; }

    /// <summary>
    /// A list of the order's line items.
    /// </summary>
    public IEnumerable<ShopifyLineItem> LineItems { get; set; } = Enumerable.Empty<ShopifyLineItem>();

    /// <summary>
    /// Returns a metafield by namespace and key that belongs to the resource.
    /// </summary>
    public ShopifyMetaField? Metafield { get; set; }

    /// <summary>
    /// List of metafields that belong to the resource.
    /// </summary>
    public IEnumerable<ShopifyMetaField> Metafields { get; set; } = Enumerable.Empty<ShopifyMetaField>();

    /// <summary>
    /// The unique identifier for the order that appears on the order page in the Shopify admin and the order status page.For example, \"#1001\", \"EN1001\", or \"1001-A\".This value isn't unique across multiple stores.
    /// </summary>
    public String Name { get; set; } = String.Empty;

    /// <summary>
    /// The net payment for the order, based on the total amount received minus the total amount refunded, in shop and presentment currencies.
    /// </summary>
    public MoneyBag NetPaymentSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The contents of the note associated with the order.
    /// </summary>
    public String? Note { get; set; }

    /// <summary>
    /// The total amount of duties before returns, in shop and presentment currencies.Returns `null` if duties aren't applicable.
    /// </summary>
    public MoneyBag? OriginalTotalDutiesSet { get; set; }

    /// <summary>
    /// The total price of the order at the time of order creation, in shop and presentment currencies.
    /// </summary>
    public MoneyBag OriginalTotalPriceSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The phone number associated with the customer.
    /// </summary>
    public String? Phone { get; set; }

    /// <summary>
    /// The fulfillment location that was assigned when the order was created.Use the [`FulfillmentOrder`](https:\/\/shopify.dev\/api\/admin-graphql\/latest\/objects\/fulfillmentorder) object for up to date fulfillment location information.
    /// </summary>
    public Location? PhysicalLocation { get; set; }

    /// <summary>
    /// The payment `CurrencyCode` of the customer for the order.
    /// </summary>
    public String PresentmentCurrencyCode { get; set; } = String.Empty;

    /// <summary>
    /// The date and time when the order was processed.This date and time might not match the date and time when the order was created.
    /// </summary>
    public DateTime ProcessedAt { get; set; }

    /// <summary>
    /// Whether the order has shipping lines or at least one line item on the order that requires shipping.
    /// </summary>
    public Boolean RequiresShipping { get; set; }

    /// <summary>
    /// The fraud risk level of the order.
    /// </summary>
    public String RiskLevel { get; set; } = String.Empty;

    /// <summary>
    /// The mailing address of the customer.
    /// </summary>
    public ShopifyMailingAddress? ShippingAddress { get; set; }

    /// <summary>
    /// A summary of all shipping costs on the order.
    /// </summary>
    public ShopifyShippingLine? ShippingLine { get; set; }

    /// <summary>
    /// A list of the order's shipping lines.
    /// </summary>
    public IEnumerable<ShopifyShippingLine> ShippingLines { get; set; } = Enumerable.Empty<ShopifyShippingLine>();

    /// <summary>
    /// A unique POS or third party order identifier.For example, \"1234-12-1000\" or \"111-98567-54\". The `receipt_number` field is derived from this value for POS orders.
    /// </summary>
    public String? SourceIdentifier { get; set; }

    /// <summary>
    /// The sum of the quantities for all line items that contribute to the order's subtotal price.
    /// </summary>
    public int SubtotalLineItemsQuantity { get; set; }

    /// <summary>
    /// The sum of the prices for all line items after discounts and before returns, in shop and presentment currencies.If `taxesIncluded` is `true`, then the subtotal also includes tax.
    /// </summary>
    public MoneyBag? SubtotalPriceSet { get; set; }

    /// <summary>
    /// A comma separated list of tags associated with the order. Updating `tags` overwritesany existing tags that were previously added to the order. To add new tags without overwritingexisting tags, use the [tagsAdd](https:\/\/shopify.dev\/api\/admin-graphql\/latest\/mutations\/tagsadd)mutation.
    /// </summary>
    public IEnumerable<String> Tags { get; set; } = Enumerable.Empty<String>();

    /// <summary>
    /// A list of all tax lines applied to line items on the order, before returns.Tax line prices represent the total price for all tax lines with the same `rate` and `title`.
    /// </summary>
    public IEnumerable<ShopifyTaxLine> TaxLines { get; set; } = Enumerable.Empty<ShopifyTaxLine>();

    /// <summary>
    /// Whether taxes are included in the subtotal price of the order.
    /// </summary>
    public Boolean TaxesIncluded { get; set; }

    /// <summary>
    /// Whether the order is a test.Test orders are made using the Shopify Bogus Gateway or a payment provider with test mode enabled.A test order cannot be converted into a real order and vice versa.
    /// </summary>
    public Boolean Test { get; set; }

    /// <summary>
    /// The authorized amount that is uncaptured or undercaptured, in shop and presentment currencies.This amount isn't adjusted for returns.
    /// </summary>
    public MoneyBag TotalCapturableSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The total amount discounted on the order before returns, in shop and presentment currencies.This includes both order and line level discounts.
    /// </summary>
    public MoneyBag? TotalDiscountsSet { get; set; }

    /// <summary>
    /// The total amount not yet transacted for the order, in shop and presentment currencies.A positive value indicates a difference in the merchant's favor (payment from customer to merchant) and a negative value indicates a difference in the customer's favor (refund from merchant to customer).
    /// </summary>
    public MoneyBag TotalOutstandingSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The total price of the order, before returns, in shop and presentment currencies.This includes taxes and discounts.
    /// </summary>
    public MoneyBag TotalPriceSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The total amount received from the customer before returns, in shop and presentment currencies.
    /// </summary>
    public MoneyBag TotalReceivedSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The total amount that was refunded, in shop and presentment currencies.
    /// </summary>
    public MoneyBag TotalRefundedSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The total amount of shipping that was refunded, in shop and presentment currencies.
    /// </summary>
    public MoneyBag TotalRefundedShippingSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The total shipping amount before discounts and returns, in shop and presentment currencies.
    /// </summary>
    public MoneyBag TotalShippingPriceSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The total tax amount before returns, in shop and presentment currencies.
    /// </summary>
    public MoneyBag? TotalTaxSet { get; set; }

    /// <summary>
    /// The sum of all tip amounts for the order, in shop and presentment currencies.
    /// </summary>
    public MoneyBag TotalTipReceivedSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The total weight of the order before returns, in grams.
    /// </summary>
    public UInt64? TotalWeight { get; set; }

    /// <summary>
    /// A list of transactions associated with the order.
    /// </summary>
    public IEnumerable<ShopifyOrderTransaction> Transactions { get; set; } = Enumerable.Empty<ShopifyOrderTransaction>();

    /// <summary>
    /// Whether no payments have been made for the order.
    /// </summary>
    public Boolean Unpaid { get; set; }

    /// <summary>
    /// The date and time when the order was modified last.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

public class ShopifyLineItem
{
    /// <summary>
    /// The line item's quantity, minus the removed quantity.
    /// </summary>
    public int CurrentQuantity { get; set; }
    /// <summary>
    /// List of additional information, often representing custom features or special requests.
    /// </summary>
    public IEnumerable<ShopifyAttribute> CustomAttributes { get; set; } = Enumerable.Empty<ShopifyAttribute>();
    /// <summary>
    /// The discounts that have been allocated onto the line item by discount applications.
    /// </summary>
    public IEnumerable<ShopifyDiscountAllocation> DiscountAllocations { get; set; } = Enumerable.Empty<ShopifyDiscountAllocation>();
    /// <summary>
    /// The total line price after discounts are applied, in shop and presentment currencies.
    /// </summary>
    public MoneyBag DiscountedTotalSet { get; set; } = MoneyBag.Empty;
    /// <summary>
    /// The approximate split price of a line item unit, in shop and presentment currencies. This value doesn't include discounts applied to the entire order.
    /// </summary>
    public MoneyBag DiscountedUnitPriceSet { get; set; } = MoneyBag.Empty;
    /// <summary>
    /// A globally-unique identifier.
    /// </summary>
    public String Id { get; set; } = String.Empty;
    /// <summary>
    /// The image associated to the line item's variant.
    /// </summary>
    public ShopifyImage? Image { get; set; }
    /// <summary>
    /// Whether the line item can be edited or not.
    /// </summary>
    public Boolean MerchantEditable { get; set; }
    /// <summary>
    /// The name of the product.
    /// </summary>
    public String Name { get; set; } = String.Empty;
    /// <summary>
    /// The total number of units that can't be fulfilled.
    /// For example, if items have been refunded, or the item is not something that can be fulfilled,
    /// like a tip.Please see the [FulfillmentOrder](https:\/\/shopify.dev\/api\/admin-graphql\/latest\/objects\/FulfillmentOrder) object for more fulfillment details.
    /// </summary>
    public int NonFulfillableQuantity { get; set; }
    /// <summary>
    /// The total price in shop and presentment currencies, without discounts applied. This value is based on the unit price of the variant x quantity.
    /// </summary>
    public MoneyBag OriginalTotalSet { get; set; } = MoneyBag.Empty;
    /// <summary>
    /// The variant unit price without discounts applied, in shop and presentment currencies.
    /// </summary>
    public MoneyBag OriginalUnitPriceSet { get; set; } = MoneyBag.Empty;
    /// <summary>
    /// The Product object associated with this line item's variant.
    /// </summary>
    public ShopifyProduct? Product { get; set; }
    /// <summary>
    /// The number of variant units ordered.
    /// </summary>
    public int Quantity { get; set; }
    /// <summary>
    /// The line item's quantity, minus the removed quantity.
    /// </summary>
    public int RefundableQuantity { get; set; }
    /// <summary>
    /// Whether physical shipping is required for the variant.
    /// </summary>
    public Boolean RequiresShipping { get; set; }
    /// <summary>
    /// Whether the line item can be restocked.
    /// </summary>
    public Boolean Restockable { get; set; }
    /// <summary>
    /// The variant SKU number.
    /// </summary>
    public String? Sku { get; set; }
    /// <summary>
    /// The taxes charged for this line item.
    /// </summary>
    public IEnumerable<ShopifyTaxLine> TaxLines { get; set; } = Enumerable.Empty<ShopifyTaxLine>();
    /// <summary>
    /// Whether the variant is taxable.
    /// </summary>
    public Boolean Taxable { get; set; }
    /// <summary>
    /// The title of the product.
    /// </summary>
    public String Title { get; set; } = String.Empty;
    /// <summary>
    /// The sum of all AppliedDiscounts on this line item, in shop and presentment currencies.
    /// </summary>
    public MoneyBag TotalDiscountSet { get; set; } = MoneyBag.Empty;
    /// <summary>
    /// The total discounted value of unfulfilled units, in shop and presentment currencies.
    /// </summary>
    public MoneyBag UnfulfilledDiscountedTotalSet { get; set; } = MoneyBag.Empty;
    /// <summary>
    /// The total price, without any discounts applied. This value is based on the unit price of the variant x quantity of all unfulfilled units, in shop and presentment currencies.
    /// </summary>
    public MoneyBag UnfulfilledOriginalTotalSet { get; set; } = MoneyBag.Empty;
    /// <summary>
    /// The number of units not yet fulfilled.
    /// </summary>
    public int UnfulfilledQuantity { get; set; }
    /// <summary>
    /// The Variant object associated with this line item.
    /// </summary>
    public ShopifyProductVariant? Variant { get; set; }
    /// <summary>
    /// The name of the variant.
    /// </summary>
    public String? VariantTitle { get; set; }
    /// <summary>
    /// The name of the vendor who made the variant.
    /// </summary>
    public String? Vendor { get; set; }
}

public static class ShopifyOrderTransactionExtensions
{
    public static IEnumerable<ShopifyOrderTransaction> Successfull(this IEnumerable<ShopifyOrderTransaction> transactions)
    {
        return transactions.Where(t => t.Status == "SUCCESS");
    }
    private static IList<string> transactionKindOrder = new List<string> { "SALE", "CAPTURE", "AUTHORIZATION" };
    public static IEnumerable<ShopifyOrderTransaction> AuthOrCapture(this IEnumerable<ShopifyOrderTransaction> transactions)
    {
        return transactions
            .Where(t => t.Kind == "SALE" || t.Kind == "AUTHORIZATION" || t.Kind == "CAPTURE")
            .OrderBy(t => transactionKindOrder.IndexOf(t.Kind));
    }
}
public class ShopifyOrderTransaction
{
    public String Id { get; set; } = String.Empty;
    public String Kind { get; set; } = String.Empty;
    public String? AccountNumber { get; set; }
    public String? AuthorizationCode { get; set; }
    public DateTime? AuthorizationExpiresAt { get; set; }
    public String? ErrorCode { get; set; }
    public String? Gateway { get; set; }
    public String? FormattedGateway { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ProcessedAt { get; set; }
    public MoneyBag AmountSet { get; set; } = MoneyBag.Empty;
    public String? ReceiptJson { get; set; }
    public String? Status { get; set; }
    public Boolean Test { get; set; }
}

public class ShopifyTaxLine
{
    /// <summary>
    /// Whether the channel that submitted the tax line is liable for remitting. A value of null indicates unknown liability for this tax line.
    /// </summary>
    public Boolean ChannelLiable { get; set; }

    /// <summary>
    /// The amount of tax, in shop and presentment currencies, after discounts and before returns.
    /// </summary>
    public MoneyBag PriceSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The proportion of the line item price that the tax represents as a decimal.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// The proportion of the line item price that the tax represents as a percentage.
    /// </summary>
    public decimal RatePercentage { get; set; }

    /// <summary>
    /// The name of the tax.
    /// </summary>
    public String Title { get; set; } = String.Empty;
}

public class ShopifyShippingLine
{
    /// <summary>
    /// A globally-unique identifier.
    /// </summary>
    public String? Id { get; set; }

    /// <summary>
    /// A reference to the carrier service that provided the rate.
    /// Present when the rate was computed by a third-party carrier service.
    /// </summary>
    public String? CarrierIdentifier { get; set; }

    /// <summary>
    /// A reference to the shipping method.
    /// </summary>
    public String? Code { get; set; }

    /// <summary>
    /// Whether the shipping line is custom or not.
    /// </summary>
    public Boolean Custom { get; set; }

    /// <summary>
    /// The general classification of the delivery method.
    /// </summary>
    public String? DeliveryCategory { get; set; }

    /// <summary>
    /// The discounts that have been allocated to the shipping line.
    /// </summary>
    public IEnumerable<ShopifyDiscountAllocation> DiscountAllocations { get; set; } = Enumerable.Empty<ShopifyDiscountAllocation>();

    /// <summary>
    /// The pre-tax shipping price with discounts applied.
    /// </summary>
    public MoneyBag DiscountedPriceSet { get; set; } = MoneyBag.Empty;


    /// <summary>
    /// The pre-tax shipping price without any discounts applied.
    /// </summary>
    public MoneyBag OriginalPriceSet { get; set; } = MoneyBag.Empty;

    /// <summary>
    /// The phone number at the shipping address.
    /// </summary>
    public String? Phone { get; set; }

    /// <summary>
    /// The fulfillment service requested for the shipping method.
    /// Present if the shipping method requires processing by a third party fulfillment service.
    /// </summary>
    public ShopifyFulfillmentService? RequestedFulfillmentService { get; set; }

    /// <summary>
    /// A unique identifier for the shipping rate. The format can change without notice and is not meant to be shown to users.
    /// </summary>
    public String? ShippingRateHandle { get; set; }

    /// <summary>
    /// Returns the rate source for the shipping line.
    /// </summary>
    public String? Source { get; set; }

    /// <summary>
    /// The TaxLine objects connected to this shipping line.
    /// </summary>
    public IEnumerable<ShopifyTaxLine> TaxLines { get; set; } = Enumerable.Empty<ShopifyTaxLine>();

    /// <summary>
    /// Returns the title of the shipping line.
    /// </summary>
    public String Title { get; set; } = String.Empty;
}

public class ShopifyDiscountAllocation
{
    /// <summary>
    /// The money amount that's allocated to a line based on the associated discount application in shop and presentment currencies.
    /// </summary>
    public MoneyBag AllocatedAmountSet { get; set; } = MoneyBag.Empty;
    /// <summary>
    /// The discount application that the allocated amount originated from.
    /// </summary>
    public ShopifyDiscountApplication DiscountApplication { get; set; } = ShopifyDiscountApplication.Empty;
}

public class ShopifyDiscountApplication
{
    public static readonly ShopifyDiscountApplication Empty = new ShopifyDiscountApplication();

    /// <summary>
    /// The method by which the discount's value is applied to its entitled items.
    /// </summary>
    public String AllocationMethod { get; set; } = String.Empty;

    /// <summary>
    /// An ordered index that can be used to identify the discount application and indicate the precedence
    /// of the discount application for calculations.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// How the discount amount is distributed on the discounted lines.
    /// </summary>
    public String TargetSelection { get; set; } = String.Empty;

    /// <summary>
    /// Whether the discount is applied on line items or shipping lines.
    /// </summary>
    public String TargetType { get; set; } = String.Empty;

    /// <summary>
    /// The value of the discount application.
    /// </summary>
    public PricingValue Value { get; set; } = PricingValue.Empty;
}

public static class ShopifyOrderExtensions
{
    public static string OrderIdFromName(this ShopifyOrder order)
    {
        return "3" + order.Name.Replace("#", "").Trim().PadLeft(7, '0');
    }
}

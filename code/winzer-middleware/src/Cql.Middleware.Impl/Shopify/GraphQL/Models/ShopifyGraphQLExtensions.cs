using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Customer;
using Cql.Middleware.Library.Shopify.Inventory;
using Cql.Middleware.Library.Shopify.Order;
using Cql.Middleware.Library.Shopify.Products;
using Cql.Middleware.Library.Shopify.Shipping;
using Cql.Middleware.Library.Shopify.Store;

namespace CQL.Middleware.Impl.Shopify.GraphQL.Models;

public static class ShopifyGraphQLExtensions
{
    #region common mappers
    public static ShopifyMetaField Map(this ShopifyGraphQLMetafield node)
    {
        return new ShopifyMetaField
        {
            Id = node.id,
            Key = node.key,
            Value = node.value,
            Namespace = node.Namespace,
        };
    }

    public static ShopifyAttribute Map(this ShopifyGraphQLAttribute node)
    {
        return new ShopifyAttribute
        {
            Key = node.key ?? String.Empty,
            Value = node.value
        };
    }

    public static ShopifyMailingAddress Map(this ShopifyGraphQLMailingAddress node)
    {
        return new ShopifyMailingAddress
        {
            Id = node.id ?? String.Empty,
            Address1 = node.address1,
            Address2 = node.address2,
            City = node.city,
            Company = node.company,
            Country = node.country,
            CountryCodeV2 = node.countryCodeV2,
            FirstName = node.firstName,
            Formatted = node.formatted ?? Enumerable.Empty<String>(),
            FormattedArea = node.formattedArea,
            LastName = node.lastName,
            Latitude = node.latitude,
            Longitude = node.longitude,
            Name = node.name,
            Phone = node.phone,
            Province = node.province,
            ProvinceCode = node.provinceCode,
            Zip = node.zip
        };
    }

    public static ShopifyImage Map(this ShopifyGraphQLImage node)
    {
        return new ShopifyImage
        {
            Id = node.id ?? String.Empty,
            AltText = node.altText,
            Height = node.height ?? 0,
            Width = node.width ?? 0,
            Url = node.url
        };
    }

    public static MoneyBag Map(this ShopifyGraphQLMoneyBag node)
    {
        return new MoneyBag
        {
            PresentmentMoney = node.presentmentMoney != null ? Map(node.presentmentMoney) : Money.Empty,
            ShopMoney = node.shopMoney != null ? Map(node.shopMoney) : Money.Empty,
        };
    }

    public static PricingValue Map(this ShopifyGraphQLPricingValue node)
    {
        return new PricingValue
        {
            Percentage = node.percentage ?? default,
            Amount = node.amount ?? default,
            CurrencyCode = node.currencyCode ?? String.Empty
        };
    }

    public static Money Map(this ShopifyGraphQLMoney node)
    {
        return new Money
        {
            Amount = node.amount ?? default,
            CurrencyCode = node.currencyCode ?? String.Empty
        };
    }

    public static EditableProperty Map(this ShopifyGraphQLEditableProperty node) {
        return new EditableProperty {
            Locked = node.locked ?? default,
            Reason = node.reason ?? String.Empty
        };
    }
    #endregion

    #region order mappers
    public static ShopifyOrder Map(this ShopifyGraphQLOrder node)
    {
        return new ShopifyOrder
        {
            Id = node.id ?? String.Empty,
            BillingAddress = node.billingAddress != null ? Map(node.billingAddress) : null,
            CancelReason = node.cancelReason,
            CancelledAt = node.cancelledAt,
            Capturable = node.capturable ?? default,
            CartDiscountAmountSet = node.cartDiscountAmountSet != null ? Map(node.cartDiscountAmountSet) : MoneyBag.Empty,
            ClientIp = node.clientIp,
            Closed = node.closed ?? default,
            ClosedAt = node.closedAt,
            Confirmed = node.confirmed ?? default,
            CreatedAt = node.createdAt ?? default,
            CurrencyCode = node.currencyCode ?? String.Empty,
            CurrentCartDiscountAmountSet = node.currentCartDiscountAmountSet != null ? Map(node.currentCartDiscountAmountSet) : MoneyBag.Empty,
            CurrentSubtotalLineItemsQuantity = node.currentSubtotalLineItemsQuantity ?? default,
            CurrentSubtotalPriceSet = node.currentSubtotalPriceSet != null ? Map(node.currentSubtotalPriceSet) : MoneyBag.Empty,
            CurrentTaxLines = node.currentTaxLines != null ? node.currentTaxLines.ToList().Select(Map) : Enumerable.Empty<ShopifyTaxLine>(),
            CurrentTotalDiscountsSet = node.currentTotalDiscountsSet != null ? Map(node.currentTotalDiscountsSet) : MoneyBag.Empty,
            CurrentTotalDutiesSet = node.currentTotalDutiesSet != null ? Map(node.currentTotalDutiesSet) : MoneyBag.Empty,
            CurrentTotalPriceSet = node.currentTotalPriceSet != null ? Map(node.currentTotalPriceSet) : MoneyBag.Empty,
            CurrentTotalTaxSet = node.currentTotalTaxSet != null ? Map(node.currentTotalTaxSet) : MoneyBag.Empty,
            CurrentTotalWeight = node.currentTotalWeight ?? default,
            CustomAttributes = node.customAttributes != null ? node.customAttributes.ToList().Select(Map) : Enumerable.Empty<ShopifyAttribute>(),
            Customer = node.customer != null ? Map(node.customer) : null,
            CustomerAcceptsMarketing = node.customerAcceptsMarketing ?? default,
            CustomerLocale = node.customerLocale,
            DiscountApplications = node.discountApplications != null ? node.discountApplications.ToList().Select(Map) : Enumerable.Empty<ShopifyDiscountApplication>(),
            DiscountCode = node.discountCode,
            DisplayAddress = node.displayAddress != null ? Map(node.displayAddress) : null,
            DisplayFinancialStatus = node.displayFinancialStatus,
            DisplayFulfillmentStatus = node.displayFulfillmentStatus,
            Edited = node.edited ?? default,
            Email = node.email,
            EstimatedTaxes = node.estimatedTaxes ?? default,
            Fulfillable = node.fulfillable ?? default,
            FulfillmentOrders = node.fulfillmentOrders != null ? node.fulfillmentOrders.ToList().Select(Map) : Enumerable.Empty<FulfillmentOrder>(),
            Fulfillments = node.fulfillments != null ? node.fulfillments.ToList().Select(Map) : Enumerable.Empty<ShopifyFulfillment>(),
            FullyPaid = node.fullyPaid ?? default,
            HasTimelineComment = node.hasTimelineComment ?? default,
            LegacyResourceId = node.legacyResourceId ?? default,
            LineItems = node.lineItems != null ? node.lineItems.ToList().Select(Map) : Enumerable.Empty<ShopifyLineItem>(),
            Metafield = node.metafield != null ? node.metafield.Map() : null,
            Metafields = node.metafields != null ? node.metafields.ToList().Select(m => m.Map()) : Enumerable.Empty<ShopifyMetaField>(),
            Name = node.name ?? String.Empty,
            NetPaymentSet = node.netPaymentSet != null ? Map(node.netPaymentSet) : MoneyBag.Empty,
            Note = node.note,
            OriginalTotalDutiesSet = node.originalTotalDutiesSet != null ? Map(node.originalTotalDutiesSet) : MoneyBag.Empty,
            OriginalTotalPriceSet = node.originalTotalPriceSet != null ? Map(node.originalTotalPriceSet) : MoneyBag.Empty,
            Phone = node.phone,
            PhysicalLocation = node.physicalLocation != null ? Map(node.physicalLocation) : null,
            PresentmentCurrencyCode = node.presentmentCurrencyCode ?? String.Empty,
            ProcessedAt = node.processedAt ?? default,
            RequiresShipping = node.requiresShipping ?? default,
            RiskLevel = node.riskLevel ?? String.Empty,
            ShippingAddress = node.shippingAddress != null ? Map(node.shippingAddress) : null,
            ShippingLine = node.shippingLine != null ? Map(node.shippingLine) : null,
            ShippingLines = node.shippingLines != null ? node.shippingLines.ToList().Select(Map) : Enumerable.Empty<ShopifyShippingLine>(),
            SourceIdentifier = node.sourceIdentifier,
            SubtotalLineItemsQuantity = node.subtotalLineItemsQuantity ?? default,
            SubtotalPriceSet = node.subtotalPriceSet != null ? Map(node.subtotalPriceSet) : MoneyBag.Empty,
            Tags = node.tags ?? Enumerable.Empty<String>(),
            TaxLines = node.taxLines != null ? node.taxLines.ToList().Select(Map) : Enumerable.Empty<ShopifyTaxLine>(),
            TaxesIncluded = node.taxesIncluded ?? default,
            Test = node.test ?? default,
            TotalCapturableSet = node.totalCapturableSet != null ? Map(node.totalCapturableSet) : MoneyBag.Empty,
            TotalDiscountsSet = node.totalDiscountsSet != null ? Map(node.totalDiscountsSet) : null,
            TotalOutstandingSet = node.totalOutstandingSet != null ? Map(node.totalOutstandingSet) : MoneyBag.Empty,
            TotalPriceSet = node.totalPriceSet != null ? Map(node.totalPriceSet) : MoneyBag.Empty,
            TotalReceivedSet = node.totalReceivedSet != null ? Map(node.totalReceivedSet) : MoneyBag.Empty,
            TotalRefundedSet = node.totalRefundedSet != null ? Map(node.totalRefundedSet) : MoneyBag.Empty,
            TotalRefundedShippingSet = node.totalRefundedShippingSet != null ? Map(node.totalRefundedShippingSet) : MoneyBag.Empty,
            TotalShippingPriceSet = node.totalShippingPriceSet != null ? Map(node.totalShippingPriceSet) : MoneyBag.Empty,
            TotalTaxSet = node.totalTaxSet != null ? Map(node.totalTaxSet) : null,
            TotalTipReceivedSet = node.totalTipReceivedSet != null ? Map(node.totalTipReceivedSet) : MoneyBag.Empty,
            TotalWeight = node.totalWeight,
            Transactions = node.transactions != null ? node.transactions.ToList().Select(Map) : Enumerable.Empty<ShopifyOrderTransaction>(),
            Unpaid = node.unpaid ?? default,
            UpdatedAt = node.updatedAt ?? default,
        };
    }

    public static ShopifyTaxLine Map(this ShopifyGraphQLTaxLine node)
    {
        return new ShopifyTaxLine
        {
            ChannelLiable = node.channelLiable ?? default,
            PriceSet = node.priceSet != null ? Map(node.priceSet) : MoneyBag.Empty,
            Rate = node.rate ?? default,
            RatePercentage = node.ratePercentage ?? default,
            Title = node.title ?? String.Empty,
        };
    }

    public static ShopifyLineItem Map(this ShopifyGraphQLLineItem node)
    {
        return new ShopifyLineItem
        {
            Id = node.id ?? String.Empty,
            CurrentQuantity = node.currentQuantity ?? default,
            CustomAttributes = node.customAttributes != null ? node.customAttributes.ToList().Select(Map) : Enumerable.Empty<ShopifyAttribute>(),
            DiscountAllocations = node.discountAllocations != null ? node.discountAllocations.ToList().Select(Map) : Enumerable.Empty<ShopifyDiscountAllocation>(),
            DiscountedTotalSet = node.discountedTotalSet != null ? Map(node.discountedTotalSet) : MoneyBag.Empty,
            DiscountedUnitPriceSet = node.discountedUnitPriceSet != null ? Map(node.discountedUnitPriceSet) : MoneyBag.Empty,
            Image = node.image != null ? Map(node.image) : null,
            MerchantEditable = node.merchantEditable ?? default,
            Name = node.name ?? String.Empty,
            NonFulfillableQuantity = node.nonFulfillableQuantity ?? default,
            OriginalTotalSet = node.originalTotalSet != null ? Map(node.originalTotalSet) : MoneyBag.Empty,
            OriginalUnitPriceSet = node.originalUnitPriceSet != null ? Map(node.originalUnitPriceSet) : MoneyBag.Empty,
            Product = node.product != null ? Map(node.product) : null,
            Quantity = node.quantity ?? default,
            RefundableQuantity = node.refundableQuantity ?? default,
            RequiresShipping = node.requiresShipping ?? default,
            Restockable = node.restockable ?? default,
            Sku = node.sku,
            TaxLines = node.taxLines != null ? node.taxLines.ToList().Select(Map) : Enumerable.Empty<ShopifyTaxLine>(),
            Taxable = node.taxable ?? default,
            Title = node.title ?? String.Empty,
            TotalDiscountSet = node.totalDiscountSet != null ? Map(node.totalDiscountSet) : MoneyBag.Empty,
            UnfulfilledDiscountedTotalSet = node.unfulfilledDiscountedTotalSet != null ? Map(node.unfulfilledDiscountedTotalSet) : MoneyBag.Empty,
            UnfulfilledOriginalTotalSet = node.unfulfilledOriginalTotalSet != null ? Map(node.unfulfilledOriginalTotalSet) : MoneyBag.Empty,
            UnfulfilledQuantity = node.unfulfilledQuantity ?? default,
            Variant = node.variant != null ? Map(node.variant) : null,
            VariantTitle = node.variantTitle ?? default,
            Vendor = node.vendor ?? default,
        };
    }

    public static ShopifyShippingLine Map(this ShopifyGraphQLShippingLine node)
    {
        return new ShopifyShippingLine
        {
            Id = node.id ?? String.Empty,
            CarrierIdentifier = node.carrierIdentifier,
            Code = node.code,
            Custom = node.custom,
            DeliveryCategory = node.deliveryCategory,
            DiscountAllocations = node.discountAllocations != null ? node.discountAllocations.ToList().Select(Map) : Enumerable.Empty<ShopifyDiscountAllocation>(),
            DiscountedPriceSet = node.discountedPriceSet != null ? Map(node.discountedPriceSet) : MoneyBag.Empty,
            OriginalPriceSet = node.originalPriceSet != null ? Map(node.originalPriceSet) : MoneyBag.Empty,
            Phone = node.phone,
            ShippingRateHandle = node.shippingRateHandle,
            Source = node.source,
            TaxLines = node.taxLines != null ? node.taxLines.ToList().Select(Map) : Enumerable.Empty<ShopifyTaxLine>(),
            Title = node.title ?? String.Empty
        };
    }

    public static ShopifyOrderTransaction Map(this ShopifyGraphQLOrderTransaction node)
    {
        return new ShopifyOrderTransaction
        {
            Id = node.id ?? String.Empty,
            Kind = node.kind ?? String.Empty,
            Gateway = node.gateway ?? String.Empty,
            FormattedGateway = node.formattedGateway ?? String.Empty,
            AmountSet = node.amountSet != null ? Map(node.amountSet) : MoneyBag.Empty,
            AccountNumber = node.accountNumber,
            AuthorizationCode = node.authorizationCode,
            AuthorizationExpiresAt = node.authorizationExpiresAt,
            CreatedAt = node.createdAt ?? default,
            ProcessedAt = node.processedAt ?? default,
            ErrorCode = node.errorCode,
            ReceiptJson = node.receiptJson,
            Status = node.status,
            Test = node.test ?? default,
        };
    }

    public static ShopifyCustomer Map(this ShopifyGraphQLCustomer node)
    {
        return new ShopifyCustomer
        {
            Id = node.id ?? String.Empty,
            // TODO
        };
    }

    public static ShopifyDiscountApplication Map(this ShopifyGraphQLDiscountApplication node)
    {
        return new ShopifyDiscountApplication
        {
            AllocationMethod = node.allocationMethod ?? String.Empty,
            Index = node.index ?? default,
            TargetSelection = node.targetSelection ?? String.Empty,
            TargetType = node.targetType ?? String.Empty,
            Value = node.value != null ? Map(node.value) : PricingValue.Empty
        };
    }

    public static ShopifyDiscountAllocation Map(this ShopifyGraphQLDiscountAllocation node)
    {
        return new ShopifyDiscountAllocation
        {
            AllocatedAmountSet = node.allocatedAmountSet != null ? Map(node.allocatedAmountSet) : MoneyBag.Empty,
            DiscountApplication = node.discountApplication != null ? Map(node.discountApplication) : ShopifyDiscountApplication.Empty
        };
    }
    #endregion

    #region inventory mappers

    public static InventoryItem Map(this ShopifyGraphQLInventoryItem node) {
        return new InventoryItem {
            Id = node.id ?? String.Empty,
            Sku = node.sku,
            CountryCodeOfOrigin = node.countryCodeOfOrigin,
            DuplicateSkuCount = node.duplicateSkuCount ?? default,
            HarmonizedSystemCode = node.harmonizedSystemCode,
            InventoryHistoryUrl = node.inventoryHistoryUrl,
            LegacyResourceId = node.legacyResourceId ?? String.Empty,
            LocationsCount = node.locationsCount ?? default,
            ProvinceCodeOfOrigin = node.provinceCodeOfOrigin,
            RequiresShipping = node.requiresShipping ?? default,
            Tracked = node.tracked ?? default,
            CreatedAt = node.createdAt ?? default,
            UpdatedAt = node.updatedAt ?? default,
            TrackedEditable = node.trackedEditable != null ? Map(node.trackedEditable) : null,
            UnitCost = node.unitCost != null ? Map(node.unitCost) : null,
            InventoryLevel = node.inventoryLevel != null ? Map(node.inventoryLevel) : null
        };
    }

    public static InventoryLevel Map(this ShopifyGraphQLInventoryLevel node) {
        return new InventoryLevel {
            Id = node.id ?? String.Empty,
            Available = node.available ?? default,
            CanDeactivate = node.canDeactivate ?? default,
            DeactivationAlert = node.deactivationAlert,
            DeactivationAlertHtml = node.deactivationAlertHtml,
            Incoming = node.incoming ?? default,
            CreatedAt = node.createdAt ?? default,
            UpdatedAt = node.updatedAt ?? default,
            Item = node.item != null ? Map(node.item) : null,
            Location = node.location != null ? Map(node.location) : null
        };
    }

    public static Location Map(this ShopifyGraphQLLocation node)
    {
        return new Location {
            Id = node.id ?? String.Empty,
            Name = node.name ??  String.Empty,
            Activatable = node.activatable ?? default,
            Deactivatable = node.deactivatable ?? default,
            DeactivatedAt = node.deactivatedAt,
            Deletable = node.deletable ?? default,
            FulfillsOnlineOrders = node.fulfillsOnlineOrders ?? default,
            HasActiveInventory = node.hasActiveInventory ?? default,
            HasUnfulfilledOrders = node.hasUnfulfilledOrders ?? default,
            IsActive = node.isActive ?? default,
            LegacyResourceId = node.legacyResourceId ?? String.Empty,
            ShipsInventory = node.shipsInventory ?? default,
            InventoryLevels = node.inventoryLevels != null ? node.inventoryLevels.ToList().Select(Map) : Enumerable.Empty<InventoryLevel>()
        };
    }
    #endregion

    #region product mappers
    public static ShopifyProduct Map(this ShopifyGraphQLProduct node)
    {
        return new ShopifyProduct
        {
            // TODO: Add more fields as needed.
            Id = node.Id,
        };
    }

    public static ShopifyProductVariant Map(this ShopifyGraphQLProductVariant node)
    {
        return new ShopifyProductVariant
        {
            // TODO: Add more fields.
            Id = node.id,
            SKU = node.sku,
            MediaId = node?.media?.edges?.FirstOrDefault()?.node?.id,
            Price = node?.price,
            Option1 = node?.selectedOptions?.Length > 0 ? node.selectedOptions[0].value : null,
            Option2 = node?.selectedOptions?.Length > 1 ? node.selectedOptions[1].value : null,
            Option3 = node?.selectedOptions?.Length > 2 ? node.selectedOptions[2].value : null,
            Metafield = node?.metafield != null ? node.metafield.Map() : null,
        };
    }
    #endregion

    #region fulfillment mappers
    public static ShopifyFulfillment Map(this ShopifyGraphQLFulfillment node)
    {
        return new ShopifyFulfillment
        {
            Id = node.id ?? String.Empty,
            FulfillmentLineItems = node.fulfillmentLineItems != null ? node.fulfillmentLineItems.ToList().Select(Map) : Enumerable.Empty<ShopifyFulfillmentLineItem>(),
            TrackingInfo = node.trackingInfo != null ? node.trackingInfo.Select(Map) : Enumerable.Empty<ShopifyFulfillmentTrackingInfo>(),
            CreatedAt = node.createdAt ?? default,
            UpdatedAt = node.updatedAt ?? default
        };
    }

    public static ShopifyFulfillmentLineItem Map(this ShopifyGraphQLFulfillmentLineItem node)
    {
        return new ShopifyFulfillmentLineItem
        {
            Id = node.id ?? String.Empty,
            Quantity = node.quantity,
            LineItem = node.lineItem != null ? Map(node.lineItem) : new ShopifyLineItem(),
        };
    }

    public static ShopifyFulfillmentTrackingInfo Map(this ShopifyGraphQLFulfillmentTrackingInfo node)
    {
        return new ShopifyFulfillmentTrackingInfo
        {
            Number = node.number,
            Company = node.company,
            Url = node.url
        };
    }

    public static FulfillmentOrder Map(this ShopifyGraphQLFulfillmentOrder node)
    {
        return new FulfillmentOrder
        {
            Id = node.id ?? String.Empty,
            LineItems = node.lineItems != null ? node.lineItems.ToList().Select(Map) : Enumerable.Empty<FulfillmentOrderLineItem>(),
            Status = node.status ?? String.Empty,
            DeliveryMethod = node.deliveryMethod != null ? Map(node.deliveryMethod) : new DeliveryMethod(),
        };
    }

    public static FulfillmentOrderLineItem Map(this ShopifyGraphQLFulfillmentOrderLineItem node)
    {
        return new FulfillmentOrderLineItem
        {
            Id = node.id ?? String.Empty,
            RemainingQuantity = node.remainingQuantity ?? 0,
            totalQuantity = node.totalQuantity ?? 0,
            LineItem = node.lineItem != null ? Map(node.lineItem) : new ShopifyLineItem()
        };
    }

    public static DeliveryMethod Map(this ShopifyGraphQLDeliveryMethod node)
    {
        return new DeliveryMethod
        {
            Id = node.id ?? String.Empty,
            MaxDeliveryDateTime = node.maxDeliveryDateTime ?? default,
            MethodType = node.methodType ?? String.Empty,
            MinDeliveryDateTime = node.minDeliveryDateTime ?? default
        };
    }
    #endregion
}


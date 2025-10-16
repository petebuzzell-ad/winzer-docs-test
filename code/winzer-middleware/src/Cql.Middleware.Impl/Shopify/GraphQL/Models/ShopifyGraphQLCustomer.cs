namespace CQL.Middleware.Impl.Shopify.GraphQL.Models;

public class ShopifyGraphQLCustomer
{
    public IEnumerable<ShopifyGraphQLMailingAddress>? addresses { get; set; }
    public ShopifyGraphQLMoney? amountSpent { get; set; }
    public ShopifyGraphQLMoney? averageOrderAmountV2 { get; set; }
    public Boolean? canDelete { get; set; }
    public DateTime? createdAt { get; set; }
    public ShopifyGraphQLMailingAddress? defaultAddress { get; set; }
    public String? displayName { get; set; }
    public String? email { get; set; }
    public ShopifyGraphQLCustomerEmailMarketingConsentState? emailMarketingConsent { get; set; }
    public String? firstName { get; set; }
    public Boolean? hasTimelineComment { get; set; }
    public String? id { get; set; }
    public ShopifyGraphQLImage? image { get; set; }
    public String? lastName { get; set; }
    public ShopifyGraphQLOrder? lastOrder { get; set; }
    public UInt64? legacyResourceId { get; set; }
    public String? lifetimeDuration { get; set; }
    public String? locale { get; set; }
    public ShopifyGraphQLMetafield? metafield { get; set; }
    public IEnumerable<ShopifyGraphQLMetafield>? metafields { get; set; }
    public String? nultipassIdentifier { get; set; }
    public String? note { get; set; }
    public UInt64? numberOfOrders { get; set; }
    public IEnumerable<ShopifyGraphQLOrder>? orders { get; set; }
    public IEnumerable<ShopifyGraphQLCustomerPaymentMethod>? paymentMethods { get; set; }
    public String? phone { get; set; }
    public String? productSubscriberStatus { get; set; }
    public ShopifyGraphQLCustomerSmsMarketingConsentState? smsMarketingConsent { get; set; }
    public String? state { get; set; }
    public IEnumerable<String>? tags { get; set; }
    public Boolean? taxExempt { get; set; }
    public IEnumerable<String>? taxExemptions { get; set; }
    public String? unsubscribeUrl { get; set; }
    public DateTime? updatedAt { get; set; }
    public Boolean? validEmailAddress { get; set; }
    public Boolean? verifiedEmail { get; set; }
}

public class ShopifyGraphQLCustomerEmailMarketingConsentState
{
    //Placeholder
}

public class ShopifyGraphQLCustomerSmsMarketingConsentState
{
    //Placeholder
}

public class ShopifyGraphQLCustomerPaymentMethod
{
    //Placeholder
}

public class ShopifyGraphQLMailingAddress {
    public String? address1 { get; set; }
    public String? address2 { get; set; }
    public String? city { get; set; }
    public String? company { get; set; }
    public String? country { get; set; }
    public String? countryCodeV2 { get; set; }
    public String? firstName { get; set; }
    public IEnumerable<String>? formatted { get; set; }
    public String? formattedArea { get; set; }
    public String? id { get; set; }
    public String? lastName { get; set; }
    public Double? latitude { get; set; }
    public Double? longitude { get; set; }
    public String? name { get; set; }
    public String? phone { get; set; }
    public String? province { get; set; }
    public String? provinceCode { get; set; }
    public String? zip { get; set; }
}

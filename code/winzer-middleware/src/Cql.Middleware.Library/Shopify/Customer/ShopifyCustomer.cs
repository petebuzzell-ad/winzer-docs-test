using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Order;

namespace Cql.Middleware.Library.Shopify.Customer;

public class ShopifyCustomer
{
    /// <summary>
    /// A list of addresses associated with the customer.
    /// </summary>
    public IEnumerable<ShopifyMailingAddress> Addresses { get; set; } = Enumerable.Empty<ShopifyMailingAddress>();
    /// <summary>
    /// The total amount that the customer has spent on orders in their lifetime.
    /// </summary>
    public Money AmountSpent { get; set; } = Money.Empty;
    /// <summary>
    /// The average amount that the customer spent per order.
    /// </summary>
    public Money? AverageOrderAmountV2 { get; set; }
    /// <summary>
    /// Whether the merchant can delete the customer from their store.
    /// A customer can be deleted from a store only if they have not yet made an order. After a customer makes an
    /// order, they can't be deleted from a store.
    /// </summary>
    public Boolean CanDelete { get; set; }
    /// <summary>
    /// The date and time when the customer was added to the store.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// The default address associated with the customer.
    /// </summary>
    public ShopifyMailingAddress? DefaultAddress { get; set; }
    /// <summary>
    /// The full name of the customer, based on the values for first_name and last_name. If the first_name and
    /// last_name are not available, then this falls back to the customer's email address, and if that is not available, the customer's phone number.
    /// </summary>
    public String DisplayName { get; set; } = String.Empty;
    /// <summary>
    /// The customer's email address.
    /// </summary>
    public String? Email { get; set; }
    /// <summary>
    /// The current email marketing state for the customer.
    /// If the customer doesn't have an email address, then this property is `null`.
    /// </summary>
    public ShopifyCustomerEmailMarketingConsentState? EmailMarketingConsent { get; set; }

    /// <summary>
    /// The customer's first name.
    /// </summary>
    public String? FirstName { get; set; }
    /// <summary>
    /// Whether the timeline subject has a timeline comment. If true, then a timeline comment exists.
    /// </summary>
    public Boolean HasTimelineComment { get; set; }
    /// <summary>
    /// A globally-unique identifier.
    /// </summary>
    public String Id { get; set; } = String.Empty;
    /// <summary>
    /// The image associated with the customer.
    /// </summary>
    public ShopifyImage Image { get; set; } = ShopifyImage.Empty;
    /// <summary>
    /// The customer's last name.
    /// </summary>
    public String? LastName { get; set; }
    /// <summary>
    /// The customer's last order.
    /// </summary>
    public ShopifyOrder? LastOrder { get; set; }
    /// <summary>
    /// The ID of the corresponding resource in the REST Admin API.
    /// </summary>
    public UInt64 LegacyResourceId { get; set; }
    /// <summary>
    /// The amount of time since the customer was first added to the store.
    /// Example: 'about 12 years'.
    /// </summary>
    public String LifetimeDuration { get; set; } = String.Empty;
    /// <summary>
    /// The customer's locale.
    /// </summary>
    public String Locale { get; set; } = String.Empty;

    /// <summary>
    /// Returns a metafield by namespace and key that belongs to the resource.
    /// </summary>
    public ShopifyMetaField? Metafield { get; set; }
    /// <summary>
    /// List of metafields that belong to the resource.
    /// </summary>
    public IEnumerable<ShopifyMetaField> Metafields { get; set; } = Enumerable.Empty<ShopifyMetaField>();
    /// <summary>
    /// A unique identifier for the customer that's used with Multipass login.
    /// </summary>
    public String? MultipassIdentifier { get; set; }
    /// <summary>
    /// A note about the customer.
    /// </summary>
    public String? Note { get; set; }
    /// <summary>
    /// The number of orders that the customer has made at the store in their lifetime.
    /// </summary>
    public UInt64 NumberOfOrders { get; set; }
    /// <summary>
    /// A list of the customer's orders.
    /// </summary>
    public IEnumerable<ShopifyOrder> Orders { get; set; } = Enumerable.Empty<ShopifyOrder>();
    /// <summary>
    /// A list of the customer's payment methods.
    /// </summary>
    public IEnumerable<CustomerPaymentMethod> PaymentMethods { get; set; } = Enumerable.Empty<CustomerPaymentMethod>();
    /// <summary>
    /// The customer's phone number.
    /// </summary>
    public String? Phone { get; set; }
    /// <summary>
    /// Returns a private metafield by namespace and key that belongs to the resource.
    /// </summary>
    public ShopifyPrivateMetafield? PrivateMetafield { get; set; }
    /// <summary>
    /// List of private metafields that belong to the resource.
    /// </summary>
    public IEnumerable<ShopifyPrivateMetafield> PrivateMetafields { get; set; } = Enumerable.Empty<ShopifyPrivateMetafield>();
    /// <summary>
    /// The current SMS marketing state for the customer's phone number.\n\nIf the customer does not have a phone number, then this property is `null`.\n
    /// </summary>
    public CustomerSmsMarketingConsentState? smsMarketingConsent { get; set; }
    /// <summary>
    /// The state of the customer's account with the shop.
    /// </summary>
    public String State { get; set; } = String.Empty;
    /// <summary>
    /// A comma separated list of tags that have been added to the customer.
    /// </summary>
    public IEnumerable<String> Tags { get; set; } = Enumerable.Empty<String>();
    /// <summary>
    /// Whether the customer is exempt from being charged taxes on their orders.
    /// </summary>
    public Boolean TaxExempt { get; set; }
    /// <summary>
    /// The list of tax exemptions applied to the customer.
    /// </summary>
    public IEnumerable<String> TaxExemptions { get; set; } = Enumerable.Empty<String>();
    /// <summary>
    /// The URL to unsubscribe the customer from the mailing list.
    /// </summary>
    public String UnsubscribeUrl { get; set; } = String.Empty;
    /// <summary>
    /// The date and time when the customer was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    /// <summary>
    /// Whether the email address is formatted correctly. This does not
    /// guarantee that the email address actually exists.
    /// </summary>
    public Boolean ValidEmailAddress { get; set; }
    /// <summary>
    /// Whether the customer has verified their email address. Defaults to `true` if the customer is created through the Shopify admin or API.
    /// </summary>
    public Boolean VerifiedEmail { get; set; }
}

public class ShopifyCustomerEmailMarketingConsentState
{
    //Placeholder
}

public class CustomerSmsMarketingConsentState
{
    //Placeholder
}

public class CustomerPaymentMethod
{
    //Placeholder
}

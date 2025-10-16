namespace Cql.Middleware.Library.Shopify.Common;

public class ShopifyMailingAddress {
    /// <summary>
    /// The first line of the address. Typically the street address or PO Box number.
    /// </summary>
    public String? Address1 { get; set; }

    /// <summary>
    /// The second line of the address. Typically the number of the apartment, suite, or unit.
    /// </summary>
    public String? Address2 { get; set; }

    /// <summary>
    /// The name of the city, district, village, or town.
    /// </summary>
    public String? City { get; set; }

    /// <summary>
    /// The name of the customer's company or organization.
    /// </summary>
    public String? Company { get; set; }

    /// <summary>
    /// The name of the country.
    /// </summary>
    public String? Country { get; set; }

    /// <summary>
    /// The two-letter code for the country of the address.
    /// For example, US.
    /// </summary>
    public String? CountryCodeV2 { get; set; }

    /// <summary>
    /// The first name of the customer.
    /// </summary>
    public String? FirstName { get; set; }

    /// <summary>
    /// A formatted version of the address, customized by the provided arguments.
    /// </summary>
    public IEnumerable<String> Formatted { get; set; } = Enumerable.Empty<String>();

    /// <summary>
    /// A comma-separated list of the values for city, province, and country.
    /// </summary>
    public String? FormattedArea { get; set; }

    /// <summary>
    /// A globally-unique identifier.
    /// </summary>
    public String Id { get; set; } = String.Empty;

    /// <summary>
    /// The last name of the customer.
    /// </summary>
    public String? LastName { get; set; }

    /// <summary>
    /// The latitude coordinate of the customer address.
    /// </summary>
    public Double? Latitude { get; set; }

    /// <summary>
    /// The longitude coordinate of the customer address.
    /// </summary>
    public Double? Longitude { get; set; }

    /// <summary>
    /// The full name of the customer, based on firstName and lastName.
    /// </summary>
    public String? Name { get; set; }

    /// <summary>
    /// A unique phone number for the customer.
    /// Formatted using E.164 standard. For example, _+16135551111_.
    /// </summary>
    public String? Phone { get; set; }

    /// <summary>
    /// The region of the address, such as the province, state, or district.
    /// </summary>
    public String? Province { get; set; }

    /// <summary>
    /// The two-letter code for the region.
    /// For example, ON.
    /// </summary>
    public String? ProvinceCode { get; set; }

    /// <summary>
    /// The zip or postal code of the address.
    /// </summary>
    public String? Zip { get; set; }
}

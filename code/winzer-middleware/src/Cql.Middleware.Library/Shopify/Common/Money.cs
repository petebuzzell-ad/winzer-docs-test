namespace Cql.Middleware.Library.Shopify.Common;

public class Money
{
    public static readonly Money Empty = new Money();
    /// <summary>
    /// Decimal money amount.
    /// </summary>
    public Decimal Amount { get; set; } = 0.0M;

    /// <summary>
    /// Currency of the money.
    /// </summary>
    public String CurrencyCode { get; set; } = String.Empty;
}

public class MoneyBag
{
    public static readonly MoneyBag Empty = new MoneyBag();
    /// <summary>
    /// Amount in presentment currency.
    /// </summary>
    public Money PresentmentMoney { get; set; } = Money.Empty;

    /// <summary>
    /// Amount in shop currency.
    /// </summary>
    public Money ShopMoney { get; set; } = Money.Empty;
}

public class PricingValue : Money
{
    public static readonly new PricingValue Empty = new PricingValue();
    /// <summary>
    /// The percentage value of the object.
    /// </summary>
    public Decimal Percentage { get; set; }
}

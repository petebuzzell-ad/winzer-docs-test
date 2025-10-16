#pragma warning disable IDE1006 // Naming Styles
namespace CQL.Middleware.Impl.Shopify.GraphQL.Models
{
    public class ShopifyGraphQLMoney
    {
        public Decimal? amount { get; set; }
        public String? currencyCode { get; set; }
    }

    public class ShopifyGraphQLMoneyBag
    {
        public ShopifyGraphQLMoney? presentmentMoney { get; set; }
        public ShopifyGraphQLMoney? shopMoney { get; set; }
    }

    public class ShopifyGraphQLPricingValue : ShopifyGraphQLMoney
    {
        public Decimal? percentage { get; set; }
    }
}

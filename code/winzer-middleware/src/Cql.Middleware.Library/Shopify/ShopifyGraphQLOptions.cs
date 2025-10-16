namespace Cql.Middleware.Library.Shopify;

public class ShopifyGraphQLOptions
{
    public static String EndPointUrl(String storeName, String apiVersion)
    {
          return $"https://{storeName}.myshopify.com/admin/api/{apiVersion}/graphql.json";
    }
    public int RetryAttempts { get; set; } = 5;
    public String StoreName { get; set; } = String.Empty;
    public String ApiVersion { get; set; } =  "2022-04";
    public String AccessTokenName { get; set; } = "X-Shopify-Access-Token";
    public String AccessTokenValue { get; set; } = String.Empty;
}

public static class ShopifyGraphQLOptionsExtensions
{
    public static String EndPointUrl(this ShopifyGraphQLOptions options)
    {
        return ShopifyGraphQLOptions.EndPointUrl(options.StoreName, options.ApiVersion);
    }
}

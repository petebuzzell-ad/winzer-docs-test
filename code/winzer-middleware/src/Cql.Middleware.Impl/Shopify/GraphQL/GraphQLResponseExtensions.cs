using GraphQL;

namespace CQL.Middleware.Impl.Shopify.GraphQL;

public static class GraphQLResponseExtensions
{
    public static bool HasErrors<T>(this GraphQLResponse<T> response)
    {
        return (response.Errors ?? Enumerable.Empty<GraphQLError>()).Any();
    }

    public static bool InternalServerError<T>(this GraphQLResponse<T> response)
    {
        return (response.Errors ?? Enumerable.Empty<GraphQLError>()).Any(e => e.InternalServerError());
    }

    public static bool ShopInactive<T>(this GraphQLResponse<T> response)
    {
        return (response.Errors ?? Enumerable.Empty<GraphQLError>()).Any(e => e.ShopInactive());
    }

    public static bool IsNotAuthenticted<T>(this GraphQLResponse<T> response)
    {
        return (response.Errors ?? Enumerable.Empty<GraphQLError>()).Any(e => e.IsNotAuthenticted());
    }

    public static bool IsThrottled<T>(this GraphQLResponse<T> response)
    {
        return (response.Errors ?? Enumerable.Empty<GraphQLError>()).Any(e => e.IsThrottled());
    }

    public static bool MaxCostExceeded<T>(this GraphQLResponse<T> response)
    {
        return (response.Errors ?? Enumerable.Empty<GraphQLError>()).Any(e => e.MaxCostExceeded());
    }

}

public static class GraphQLErrorExtensions
{
    private const string _code = "code";

    public static String? Code(this GraphQLError error)
    {
        return (error?.Extensions?.TryGetValue(_code, out var value) ?? false) ? value.ToString() : null;
    }

    public static bool InternalServerError(this GraphQLError error)
    {
        return String.Equals("INTERNAL_SERVER_ERROR", error.Code(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool ShopInactive(this GraphQLError error)
    {
        return String.Equals("SHOP_INACTIVE", error.Code(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsNotAuthenticted(this GraphQLError error)
    {
        return String.Equals("ACCESS_DENIED", error.Code(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsThrottled(this GraphQLError error)
    {
        return String.Equals("THROTTLED", error.Code(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsTimeout(this GraphQLError error)
    {
        return String.Equals("TIMEOUT", error.Code(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool MaxCostExceeded(this GraphQLError error)
    {
        return String.Equals("MAX_COST_EXCEEDED", error.Code(), StringComparison.OrdinalIgnoreCase);
    }
}

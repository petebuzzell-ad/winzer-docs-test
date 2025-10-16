
using GraphQL;

namespace CQL.Middleware.Impl.Shopify.GraphQL;

public static class GraphQLMapExtensions
{
    public static Map? Dig(this Map? hash, String key)
    {
        if (hash != null && hash.ContainsKey(key))
        {
            return hash[key] as Map;
        }
        return default(Map);
    }
}

namespace CQL.Middleware.Impl.Shopify.GraphQL.Models
{
#pragma warning disable IDE1006 // Naming Styles
    public class ShopifyGraphQLResultPage<TNode>
    {

        public ShopifyGraphQLResultEdge<TNode>[]? edges { get; set; }

        public ShopifyGraphQLPageInfo pageInfo { get; set; } = new ShopifyGraphQLPageInfo();

        public List<TNode> ToList() {
            return (this?.edges ?? Array.Empty<ShopifyGraphQLResultEdge<TNode>>())
                .Select(e => e.node)
                .Where(e => e != null)
                .OfType<TNode>()
                .ToList();
        }
    }

    public class ShopifyGraphQLResultEdge<TNode>
    {
        public TNode? node { get; set; }
    }

    public class ShopifyGraphQLPageInfo
    {
        public bool hasNextPage { get; set; } = false;

        public string? endCursor { get; set; }
    }

    public class ShopifyGraphQLEditableProperty
    {
        public Boolean? locked { get; set; }
        public String? reason { get; set; }
    }

    public class ShopifyGraphQLUserError
    {
        public IEnumerable<String>? field { get; set; }
        public String? message { get; set; }
    }

    public class ShopifyGraphQLMediaUserError : ShopifyGraphQLUserError
    {
        public string code { get; set; }
    }

    public class ShopifyGraphQLAttribute {
        public String? key { get; set; }
        public String? value { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}


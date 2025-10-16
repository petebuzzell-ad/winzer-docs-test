using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable IDE1006 // Naming Styles
namespace CQL.Middleware.Impl.Shopify.GraphQL.Models
{
    public class ShopifyGraphQLCollectionsQueryResultData
    {
        public ShopifyGraphQLResultPage<ShopifyGraphQLCollection>? collections { get; set; }
    }
    public class ShopifyGraphQLCollectionByHandleQueryResultData
    {
        public ShopifyGraphQLCollection? collectionByHandle { get; set; }
    }

    public class ShopifyGraphQLCollectionCreateResult
    {
        public ShopifyGraphQLCollectionCreate? collectionCreate { get; set; }
    }
    public class ShopifyGraphQLCollectionCreate
    {
        public ShopifyGraphQLCollection? collection { get; set; }

        public IEnumerable<ShopifyGraphQLUserError> userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLCollection
    {
        public string Id { get; set; }

        public string? Handle { get; set; }

        public string? Title { get; set; }

        public string? DescriptionHtml { get; set; }

        public string? SortOrder { get; set; }

        public ShopifyGraphQLCollectionRuleSet RuleSet { get; set; }
    }

    public class ShopifyGraphQLCollectionRuleSet
    {
        public bool AppliedDisjunctively { get; set; }

        public ShopifyGraphQLCollectionRule[]? Rules { get; set; }
    }

    public class ShopifyGraphQLCollectionRule
    {
        public string Column { get; set; }

        public string Condition { get; set; }

        public string? ConditionObjectId { get; set; }

        public string Relation { get; set; }
    }
}

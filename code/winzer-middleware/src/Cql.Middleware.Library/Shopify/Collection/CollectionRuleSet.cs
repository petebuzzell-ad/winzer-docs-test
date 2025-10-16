using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Inventory;
using Cql.Middleware.Library.Shopify.Products;

namespace Cql.Middleware.Library.Shopify.Collection
{
    public class CollectionRuleSet
    {
        /// <summary>
        /// Whether products must match any or all of the rules to be included in the collection.
        /// If true, then products must match at least one of the rules to be included in the collection.
        /// If false, then products must match all of the rules to be included in the collection.
        /// </summary>
        public Boolean appliedDisjunctively { get; set; }

        /// <summary>
        /// The rules used to assign products to the collection.
        /// </summary>
        public IList<CollectionRule> rules { get; set; } = new List<CollectionRule>();
    }
}

using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Products;

namespace Cql.Middleware.Library.Shopify.Collection
{
    public class CollectionItem
    {
        /// <summary>
        /// Specifies the collection to update or create a new collection if absent. Required for updating a collection.
        /// </summary>
        public String? id { get; set; }

        /// <summary>
        /// A unique human-friendly string for the collection. Automatically generated from the collection's title.
        /// </summary>
        public String? handle { get; set; }

        /// <summary>
        /// The title of the collection.
        /// </summary>
        public String? title { get; set; }

        /// <summary>
        /// The rules used to assign products to the collection.
        /// </summary>
        public CollectionRuleSet? ruleSet { get; set; }
    }
}

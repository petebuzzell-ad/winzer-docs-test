using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Products;

namespace Cql.Middleware.Library.Shopify.Collection
{
    public class CollectionRule
    {
        /// <summary>
        /// The attribute that the rule focuses on. For example, title or product_type.
        /// </summary>
        public String? column { get; set; }

        /// <summary>
        /// The value that the operator is applied to. For example, Hats.
        /// </summary>
        public String? condition { get; set; }

        /// <summary>
        /// The object ID that points to additional attributes for the collection rule. This is only required when using metafield definition rules.
        /// </summary>
        public String? conditionObjectId { get; set; }

        /// <summary>
        /// The type of operator that the rule is based on. For example, equals, contains, or not_equals.
        /// </summary>
        public String? relation { get; set; }
    }
}

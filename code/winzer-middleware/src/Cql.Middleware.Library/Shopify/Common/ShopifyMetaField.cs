using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Shopify.Common
{
    public class ShopifyMetaField
    {
        public string? Id { get; set; }

        public DateTimeOffset? CreatedAt
        {
            get;
            set;
        }

        public DateTimeOffset? UpdatedAt
        {
            get;
            set;
        }

        public string? Key
        {
            get;
            set;
        }

        public string? Value
        {
            get;
            set;
        }

        /// <summary>
        /// See: https://shopify.dev/apps/metafields/types
        /// </summary>
        public string? Type
        {
            get;
            set;
        }

        public string? Namespace
        {
            get;
            set;
        }

        public string? Description
        {
            get;
            set;
        }

        public string? OwnerId
        {
            get;
            set;
        }

        public string? OwnerResource
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Specifies the input fields for a metafield value to set.
    /// </summary>
    public class ShopifyMetafieldsSetInput
    {
        /// <summary>
        /// The ID of the owner resource.
        /// </summary>
        public string OwnerId { get; set; } = String.Empty;
        /// <summary>
        /// A container for a group of metafields.\nGrouping metafields in a namespace prevents your metafields from conflicting with other metafields that have the same key name.
        /// </summary>
        public string Namespace { get;set; } = String.Empty;
        /// <summary>
        /// The key name of the metafield.
        /// </summary>
        public string Key { get; set; } = String.Empty;
        /// <summary>
        /// SpThe data to store in the metafield. The data is always stored as a string, regardless of the metafield's type.
        /// </summary>
        public string Value { get; set; } = String.Empty;
        /// <summary>
        /// The type of data that the metafield stores.
        /// The type of data must be a [supported type](https://shopify.dev/apps/metafields/types)
        /// </summary>
        public string Type { get; set; } = String.Empty;
    }
}

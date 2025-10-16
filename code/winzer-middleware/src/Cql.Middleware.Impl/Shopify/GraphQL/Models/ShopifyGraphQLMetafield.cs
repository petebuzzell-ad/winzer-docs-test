using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CQL.Middleware.Impl.Shopify.GraphQL.Models
{
    // Using DataContract here because of the "namespace" property which is a reserved keyword.
    [DataContract]
    public class ShopifyGraphQLMetafield
    {
        [DataMember]
        public string? id { get; set; }

        [DataMember]
        public string? key { get; set; }

        [DataMember]
        public string? value { get; set; }

        [DataMember(Name = "namespace")]
        public string? Namespace { get;set; }
    }

    [DataContract]
    public class ShopifyGraphQLMetafieldsSetInput
    {
        [DataMember]
        public string ownerId { get; set; } = String.Empty;

        public string @namespace { get;set; } = String.Empty;
        [DataMember]
        public string key { get; set; } = String.Empty;
        [DataMember]
        public string value { get; set; } = String.Empty;
        [DataMember]
        public string type { get; set; } = String.Empty;
    }

    public class ShopifyGraphQLMetafieldsSetPayload
    {
        public IEnumerable<ShopifyGraphQLMetafield> metafields { get; set; } = Enumerable.Empty<ShopifyGraphQLMetafield>();

        public IEnumerable<ShopifyGraphQLUserError> userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLMetafieldDeleteResult
    {
        public ShopifyGraphQLMetafieldDeleteData? metafieldDelete { get; set; }

        public bool IsSuccessful()
        {
            if (metafieldDelete == null) return false;

            if (metafieldDelete.userErrors != null && metafieldDelete.userErrors.Any()) return false;

            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return metafieldDelete?.userErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLMetafieldDeleteData
    {
        public string deletedId { get; set; } = String.Empty;
        public IEnumerable<ShopifyGraphQLUserError> userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLMetafieldQueryResultData
    {
        public ShopifyGraphQLResultPage<ShopifyGraphQLMetafield>? metafieldDefinitions { get; set; }
    }
}

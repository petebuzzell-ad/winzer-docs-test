using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQL.Middleware.Impl.Shopify.GraphQL.Models
{
    public class ShopifyGraphQLMedia
    {
        public string? id { get; set; }

        public string? alt { get; set; }

        public string? mediaContentType { get; set; }

        public ShopifyGraphQLMediaPreviewImage? preview { get; set; }

    }

    public class ShopifyGraphQLMediaPreviewImage
    {
        public ShopifyGraphQLImage? image { get; set; }
    }

    public class ShopifyGraphQLImage
    {
        public string? id { get; set; }

        public string? altText { get; set; }

        public string? url { get; set; }

        public int? height { get; set; }

        public int? width { get; set; }

        public ShopifyGraphQLResultPage<ShopifyGraphQLMetafield> metafields { get; set; }
    }



    public class ShopifyGraphQLProductDeleteMediaResult
    {
        public ShopifyGraphQLProductDeleteMediaData? productDeleteMedia { get; set; }

        public bool IsSuccessful()
        {
            if (productDeleteMedia == null) return false;

            if (productDeleteMedia.mediaUserErrors != null && productDeleteMedia.mediaUserErrors.Any()) return false;

            return true;
        }

        public IList<ShopifyGraphQLUserError> ErrorList()
        {
            return productDeleteMedia?.mediaUserErrors?.ToList() ?? new List<ShopifyGraphQLUserError>();
        }
    }

    public class ShopifyGraphQLProductDeleteMediaData
    {
        public IEnumerable<ShopifyGraphQLUserError> mediaUserErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLProductCreateMediaResult
    {
        public ShopifyGraphQLProductCreateMediaResultData? productCreateMedia { get; set; }

        public bool IsSuccessful()
        {
            if (productCreateMedia == null || productCreateMedia.media == null) return false;

            if (productCreateMedia.mediaUserErrors != null && productCreateMedia.mediaUserErrors.Any()) return false;

            return true;
        }

        public IList<ShopifyGraphQLMediaUserError> ErrorList()
        {
            return productCreateMedia?.mediaUserErrors?.ToList() ?? new List<ShopifyGraphQLMediaUserError>();
        }
    }

    public class ShopifyGraphQLProductCreateMediaResultData
    {
        public ShopifyGraphQLMedia[]? media { get; set; }

        public IEnumerable<ShopifyGraphQLMediaUserError> mediaUserErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLMediaUserError>();
    }
}

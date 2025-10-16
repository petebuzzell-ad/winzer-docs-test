using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Impl.Shopify.GraphQL.Models
{

    public class ShopifyGraphQLCatalogContextUpdateResultData
    {
        public IEnumerable<ShopifyGraphQLUserError>? userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLCatalogContextUpdateResult
    {
        public ShopifyGraphQLCatalogContextUpdateResultData? catalogContextUpdate { get; set; }

        public bool IsSuccessful()
        {
            if (catalogContextUpdate == null) return false;

            if (catalogContextUpdate.userErrors != null && catalogContextUpdate.userErrors.Any()) return false;

            return true;
        }
    }

    public class ShopifyGraphQLCatalogDeleteResultData
    {
        public IEnumerable<ShopifyGraphQLUserError>? userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLCatalogDeleteResult
    {
        public ShopifyGraphQLCatalogDeleteResultData? catalogDelete { get; set; }

        public bool IsSuccessful()
        {
            if (catalogDelete == null) return false;

            if (catalogDelete.userErrors != null && catalogDelete.userErrors.Any()) return false;

            return true;
        }
    }

    public class ShopifyGraphQLCatalog
    {
        public string? id { get; set; }

        public string? title { get; set; }

        public string? status { get; set; }

        public ShopifyGraphQLPriceList? priceList { get; set; }

        public ShopifyGraphQLPublication? publication { get; set; }
    }

    public class ShopifyGraphQLCatalogQueryResultData
    {
        public ShopifyGraphQLResultPage<ShopifyGraphQLCatalog>? catalogs { get; set; }
    }
    public class ShopifyGraphQLCreateCatalogResult
    {
        public ShopifyGraphQLCreateCatalogResultData? catalogCreate { get; set; }

        public bool IsSuccessful()
        {
            if (catalogCreate == null || catalogCreate.catalog == null || string.IsNullOrWhiteSpace(catalogCreate.catalog.id)) return false;

            if (catalogCreate.userErrors != null && catalogCreate.userErrors.Any()) return false;

            return true;
        }
    }

    public class ShopifyGraphQLCreateCatalogResultData
    {
        public ShopifyGraphQLCatalog? catalog { get; set; }
        public IEnumerable<ShopifyGraphQLUserError>? userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();

    }

    public class ShopifyGraphQLPublication
    {
        public string id { get; set; }
    }

    public class ShopifyGraphQLCreatePublicationResult
    {
        public ShopifyGraphQLCreatePublicationResultData? publicationCreate { get; set; }

        public bool IsSuccessful()
        {
            if (publicationCreate == null || publicationCreate.publication == null || string.IsNullOrWhiteSpace(publicationCreate.publication.id)) return false;

            if (publicationCreate.userErrors != null && publicationCreate.userErrors.Any()) return false;

            return true;
        }
    }

    public class ShopifyGraphQLCreatePublicationResultData
    {
        public ShopifyGraphQLPublication? publication { get; set; }
        public IEnumerable<ShopifyGraphQLUserError>? userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();
    }

    public class ShopifyGraphQLPublicationUpsertResult
    {
        public ShopifyGraphQLPublicationUpsertResultData? publicationUpdate { get; set; }

        public bool IsSuccessful()
        {
            if (publicationUpdate == null) return false;

            if (publicationUpdate.userErrors != null && publicationUpdate.userErrors.Any()) return false;

            return true;
        }
    }

    public class ShopifyGraphQLPublicationUpsertResultData
    {
        public IEnumerable<ShopifyGraphQLUserError>? userErrors { get; set; } = Enumerable.Empty<ShopifyGraphQLUserError>();

    }

}

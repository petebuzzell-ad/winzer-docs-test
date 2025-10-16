using Cql.Middleware.Impl.Shopify.GraphQL.Models;
using Cql.Middleware.Library.Shopify;
using Cql.Middleware.Library.Shopify.Catalog;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Cql.Middleware.Impl.Shopify.Services
{
    public class CatalogService : BaseService, ICatalogService
    {
        public CatalogService(ILogger<CatalogService> logger, ShopifyGraphQLOptions options) : base(logger, options)
        {

        }

        public async Task<string> CreatePublicationForCatalog(string catalogId, bool autoAddNewProducts = false, bool includeAllProducts = false)
        {
            var input = new
            {
                autoPublish = autoAddNewProducts,
                catalogId = catalogId,
                defaultState = includeAllProducts ? "ALL_PRODUCTS" : "EMPTY",
            };

            var query = @"
mutation publicationCreate($input: PublicationCreateInput!) {
    publicationCreate(input: $input) {
        publication {
            id
        }
        userErrors {
            field
            message
        }
    }
}
";
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLCreatePublicationResult>(query, new { input }, false);
            if (result == null || !result.IsSuccessful())
            {
                _logger.LogError($"Error creating publication for catalog {catalogId}.  Details below if available.");
                if (result?.publicationCreate?.userErrors?.Any() ?? false)
                {
                    foreach (var userError in result.publicationCreate.userErrors)
                    {
                        _logger.LogError("{0} - {1}", userError.field?.FirstOrDefault(), userError.message);
                    }
                }

                throw new Exception("Failed to create publication.  See previous messages for error details.");
            }
            else
            {
                return result.publicationCreate.publication.id;
            }
        }


        public async Task<bool> AddCatalogToCompanyLocations(string catalogId, IEnumerable<string> companyLocationIds)
        {
            bool success = true;
            var locationsInChunks = companyLocationIds.Chunk(50);

            foreach (var locationIdChunk in locationsInChunks)
            {
                var input = new
                {
                    companyLocationIds = locationIdChunk
                };

                var query = @"
mutation catalogContextUpdate($catalogId: ID!, $contextsToAdd: CatalogContextInput) {
    catalogContextUpdate(catalogId: $catalogId, contextsToAdd: $contextsToAdd) {
        userErrors {
            field
            message
        }
    }
}
";

                var result = await ExecuteGraphQLQuery<ShopifyGraphQLCatalogContextUpdateResult>(query, new { catalogId = catalogId, contextsToAdd = input });

                if (result == null || !result.IsSuccessful())
                {
                    _logger.LogError("Error associating catalog with company locations.  Details below if available.");
                    if (result?.catalogContextUpdate?.userErrors?.Any() ?? false)
                    {
                        foreach (var userError in result.catalogContextUpdate.userErrors)
                        {
                            _logger.LogError("{0} - {1}", userError.field?.FirstOrDefault(), userError.message);
                        }
                    }

                    success = false;
                }
            }

            return success;
        }

        public async Task<string?> CreateCatalog(IEnumerable<string> companyLocationIds, ShopifyCompanyLocationCatalog catalog)
        {
            var input = new
            {
                status = "ACTIVE",
                title = catalog.Title,
                priceListId = catalog.PriceListId,
                context = new
                {
                    companyLocationIds = companyLocationIds.ToArray(),
                }
            };

            var query = @"
mutation catalogCreate($input: CatalogCreateInput!) {
    catalogCreate(input: $input) {
        catalog {
            id
        }
        userErrors {
            field
            message
        }
    }
}
";

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLCreateCatalogResult>(query, new { input }, false);
            if (result == null || !result.IsSuccessful())
            {
                _logger.LogError("Error creating catalog with title {0}.  Details below if available.", catalog.Title);
                if (result?.catalogCreate?.userErrors?.Any() ?? false) { 
                    foreach (var userError in result.catalogCreate.userErrors)
                    {
                        _logger.LogError("{0} - {1}", userError.field?.FirstOrDefault(), userError.message);
                    }
                }

                return null;
            }
            else
            {
                return result.catalogCreate.catalog.id;
            }
        }

        public async Task<bool> DeleteCatalog(string catalogId)
        {
            var query = @"
mutation catalogDelete($id: ID!) {
    catalogDelete(id: $id, deleteDependentResources: true) {
        userErrors {
          field
          message
        }
    }
}
";
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLCatalogDeleteResult>(query, new { id = catalogId });
            if (result == null || !result.IsSuccessful())
            {
                _logger.LogError("Error deleting catalog with id {0}.  Details below if available.", catalogId);
                if (result?.catalogDelete?.userErrors?.Any() ?? false)
                {
                    foreach (var userError in result.catalogDelete.userErrors)
                    {
                        _logger.LogError("{0} - {1}", userError.field?.FirstOrDefault(), userError.message);
                    }
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<IEnumerable<ShopifyCompanyLocationCatalog>> GetAllCatalogs()
        {
            return await GetAllCatalogs(null);
        }

        private async Task<IEnumerable<ShopifyCompanyLocationCatalog>> GetAllCatalogs(string? after)
        {
            var query = @"
query GetCatalogs($after: String) {
    catalogs(first: 100, after: $after) {
        edges {
            node {
                id
                title
            }
        }
        pageInfo {
            ...pageInfoFields
        }
    }
}

fragment pageInfoFields on PageInfo {
    hasNextPage
    endCursor    
}  
"
            ;

            var rval = new List<ShopifyCompanyLocationCatalog>();
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLCatalogQueryResultData>(query, new { after });
            if (result?.catalogs?.edges?.Any() ?? false)
            {
                foreach ( var edge in result.catalogs.edges)
                {
                    rval.Add(AssembleShopifyCatalog(edge.node));
                }
            }

            if (result?.catalogs?.pageInfo?.hasNextPage ?? false)
            {
                rval.AddRange(await GetAllCatalogs(result.catalogs.pageInfo.endCursor));
            }

            return rval;
        }

        public async Task<ShopifyCompanyLocationCatalog> GetCatalogByName(string catalogName, CancellationToken cancellationToken = default)
        {
            var query = @"
query GetCatalogs($query: String) {
    catalogs(first: 1, query: $query) {
        edges {
            node {
                id
                title
                priceList{
                    id
                    name
                }
                publication{
                    id
                }
            }
        }
        pageInfo {
            ...pageInfoFields
        }
    }
}

fragment pageInfoFields on PageInfo {
    hasNextPage
    endCursor    
}  
";
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLCatalogQueryResultData>(query, new { query = string.Format("title:{0}", catalogName) }, true, cancellationToken);
            if (result?.catalogs?.edges?.Any() ?? false)
            {
                return AssembleShopifyCatalog(result.catalogs.edges.First().node);
            }

            return null;
        }

        public async Task UpsertProductsToCatalogPublication(string publicationId, IEnumerable<string> productIds)
        {
            var chunks = productIds.Chunk(50);
            foreach (var chunk in chunks)
            {
                await UpsertProductsToCatalogPublication_Internal(publicationId, chunk);
            }
        }

        private async Task UpsertProductsToCatalogPublication_Internal(string publicationId, IEnumerable<string> productIds)
        {
            var input = new
            {
                publishablesToAdd = productIds.ToArray()
            };

            var query = @"
mutation publicationUpdate($id: ID!, $input: PublicationUpdateInput!) {
    publicationUpdate(id: $id, input: $input) {
        userErrors {
            field
            code
            message
        }
    }
}
";

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLPublicationUpsertResult>(query, new { id = publicationId, input });
            if (result == null || !result.IsSuccessful())
            {
                _logger.LogError("Error adding items to publication with id {0}.  Details below if available.", publicationId);
                if (result?.publicationUpdate?.userErrors?.Any() ?? false)
                {
                    foreach (var userError in result.publicationUpdate.userErrors)
                    {
                        _logger.LogError("{0} - {1}", userError.field?.FirstOrDefault(), userError.message);
                    }
                }

                throw new Exception(string.Format("Failed to add items to publication with id {0}.  See earlier log messages for details.", publicationId));
            }
        }

        private ShopifyCompanyLocationCatalog? AssembleShopifyCatalog(ShopifyGraphQLCatalog? input)
        {
            if (input == null) return null;

            var catalog = new ShopifyCompanyLocationCatalog()
            {
                Status = input.status,
                Title = input.title,
                Id = input.id
            };

            if (input.priceList != null)
            {
                catalog.PriceListId = input.priceList.id;
                catalog.PriceListName = input.priceList.name;
            }

            if (input.publication != null)
            {
                catalog.PublicationId = input.publication.id;
            }

            return catalog;
        }
    }
}

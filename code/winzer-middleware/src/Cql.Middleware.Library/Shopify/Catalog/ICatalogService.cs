using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Shopify.Catalog
{
    public interface ICatalogService
    {
        public Task<ShopifyCompanyLocationCatalog> GetCatalogByName(string catalogName, CancellationToken cancellationToken = default);
        public Task<string?> CreateCatalog(IEnumerable<string> companyLocationIds, ShopifyCompanyLocationCatalog catalog);

        public Task<string?> CreatePublicationForCatalog(string catalogId, bool autoAddNewProducts = false, bool includeAllProducts = false);

        public Task<bool> AddCatalogToCompanyLocations(string catalogId, IEnumerable<string> companyLocationIds);

        public Task<IEnumerable<ShopifyCompanyLocationCatalog>> GetAllCatalogs();

        public Task<bool> DeleteCatalog(string catalogId);

        public Task UpsertProductsToCatalogPublication(string publicationId, IEnumerable<string> productIds);
    }
}

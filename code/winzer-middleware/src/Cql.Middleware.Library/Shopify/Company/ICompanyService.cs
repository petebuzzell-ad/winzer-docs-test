using Cql.Middleware.Library.Shopify.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Library.Shopify.Company
{
    public interface ICompanyService
    {
        public Task<ShopifyCompaniesQueryResult> GetAllCompanies(string cursor = "", CancellationToken cancellationToken = default);

        public Task<ShopifyCompany> GetCompanyByExternalId(string externalId, CancellationToken cancellationToken = default);

        public Task<ShopifyCompanyLocation> GetCompanyLocationByExternalId(string externalId, CancellationToken cancellationToken = default);

        public Task<IEnumerable<ShopifyCompanyLocation>> GetLocationsForCompany(string companyId, CancellationToken cancellationToken = default);
    }

    public class ShopifyCompaniesQueryResult
    {
        public bool HasMoreResults { get; set; }

        public string? Cursor { get; set; }

        public IList<ShopifyCompany>? Companies { get; set; }
    }
}

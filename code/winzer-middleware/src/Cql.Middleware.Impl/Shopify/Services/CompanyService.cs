using Cql.Middleware.Impl.Shopify.GraphQL.Models;
using Cql.Middleware.Library.Shopify;
using Cql.Middleware.Library.Shopify.Collection;
using Cql.Middleware.Library.Shopify.Company;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using Dasync.Collections;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Impl.Shopify.Services
{
    public class CompanyService : BaseService, ICompanyService
    {
        public CompanyService(ILogger<CompanyService> logger, ShopifyGraphQLOptions options) : base(logger, options)
        {

        }

        public async Task<ShopifyCompaniesQueryResult> GetAllCompanies(string cursor = "", CancellationToken cancellationToken = default)
        {
            var afterCursor = "";
            if (!string.IsNullOrWhiteSpace(cursor))
            {
                afterCursor = $"after:\"{cursor}\"";
            }

            var query = $@"
query {{
    companies(first: 250, {afterCursor}) {{
        edges {{
            node {{
                id
                name
                externalId
                createdAt
                pricingContract: metafield(namespace: ""cql"", key: ""pricing_contract"") {{
                    value
                }}
                templateName: metafield(namespace: ""cql"", key: ""template_pricing"") {{
                    value
                }}
            }}
        }}
        pageInfo {{
            ...pageInfoFields
        }}
    }}
}}

fragment pageInfoFields on PageInfo {{
    hasNextPage
    endCursor    
}}
";
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLCompanyQueryResultData>(query, new {}, true, cancellationToken);
            var companies = new ConcurrentBag<ShopifyCompany>();
            var allCompanies = new List<ShopifyCompany>();

            if (result.companies != null && result.companies.edges != null)
            {
                foreach ( var company in result.companies.edges) {
                    if (company.node != null)
                    {
                        var shopifyCompany = new ShopifyCompany()
                        {
                            Id = company.node.id,
                            Name = company.node.name,
                            ExternalId = company.node.externalId,
                            PricingContract = company.node.pricingContract?.value,
                            TemplateName = company.node.templateName?.value
                        };

                        companies.Add(shopifyCompany);
                    }
                }

                allCompanies = companies.ToList();

                if (result.companies.pageInfo.hasNextPage && !string.IsNullOrWhiteSpace(result.companies.pageInfo.endCursor))
                {
                    var endCursor = result.companies.pageInfo.endCursor;
                    var nextCollectionsPage = await GetAllCompanies(endCursor);
                    if (nextCollectionsPage?.Companies != null && nextCollectionsPage.Companies.Any())
                    {
                        allCompanies = allCompanies.Concat(nextCollectionsPage.Companies).ToList();
                    }
                }
            }

            var rval = new ShopifyCompaniesQueryResult()
            {
                HasMoreResults = result.companies?.pageInfo?.hasNextPage ?? false,
                Cursor = result.companies?.pageInfo?.endCursor,
                Companies = allCompanies
            };

            return rval;
        }

        public async Task<ShopifyCompany?> GetCompanyByExternalId(string externalId, CancellationToken cancellationToken = default)
        {
            var query = @"
query GetCompanies($query: String) {
    companies(first: 1, query: $query) {
        edges {
            node {
                id
                name
                externalId
                createdAt
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
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLCompanyQueryResultData>(query, new { query = string.Format("external_id:\"{0}\"", externalId) }, true, cancellationToken);
            if (result?.companies?.edges?.Any() ?? false)
            {
                return AssembleShopifyCompany(result.companies.edges.First().node);
            }

            return null;
        }

        public async Task<ShopifyCompanyLocation?> GetCompanyLocationByExternalId(string externalId, CancellationToken cancellationToken = default)
        {
            var query = @"
query GetCompanyLocations($query: String) {
    companyLocations(first: 1, query: $query) {
        edges {
            node {
                id
                name
                externalId
                createdAt
                company {
                    id
                    externalId
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
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLCompanyLocationQueryResultData>(query, new { query = string.Format("external_id:\"{0}\"", externalId) }, true, cancellationToken);
            if (result?.companyLocations?.edges?.Any() ?? false)
            {
                return AssembleShopifyCompanyLocation(result.companyLocations.edges.First().node);
            }

            return null;
        }

        public async Task<IEnumerable<ShopifyCompanyLocation>> GetLocationsForCompany(string companyId, CancellationToken cancellationToken = default)
        {
            var query = @"
query GetCompanyLocations($query: String, $after: String) {
    companyLocations(first: 25, after: $after, query: $query) {
        edges {
            node {
                id
                name
                externalId
                createdAt
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
            var rval = new List<ShopifyCompanyLocation>();
            string? after = null;
            var hasNextPage = false;

            do
            {
                hasNextPage = false;
                var queryParams = new
                {
                    query = string.Format("company_id:{0}", companyId),
                    after = after,
                };

                var result = await ExecuteGraphQLQuery<ShopifyGraphQLCompanyLocationQueryResultData>(query, queryParams, true, cancellationToken);
                var locations = result?.companyLocations;
                if (locations != null)
                {
                    rval.AddRange(locations.edges.Select(e => AssembleShopifyCompanyLocation(e.node)));

                    hasNextPage = locations.pageInfo.hasNextPage;
                    if (hasNextPage) after = locations.pageInfo.endCursor;
                }
            } while (hasNextPage);

            return rval;
        }

        private ShopifyCompany? AssembleShopifyCompany(ShopifyGraphQLCompany? input)
        {
            if (input == null) return null;

            return new ShopifyCompany()
            {
                CreatedAt = input.createdAt,
                Name = input.name,
                ExternalId = input.externalId,
                Id = input.id
            };
        }

        private ShopifyCompanyLocation AssembleShopifyCompanyLocation(ShopifyGraphQLCompanyLocation input)
        {
            return new ShopifyCompanyLocation()
            {
                Name = input.name,
                ExternalId = input.externalId,
                Id = input.id,
                CompanyExternalId = input.company?.externalId,
                CompanyId = input.company?.id           
            };
        }
    }
}

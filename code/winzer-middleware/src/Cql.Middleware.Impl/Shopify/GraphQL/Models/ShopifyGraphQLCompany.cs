using Cql.Middleware.Library.Shopify.Common;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Impl.Shopify.GraphQL.Models
{
    public class ShopifyGraphQLCompany
    {
        public string? id { get; set; }

        public string? name { get; set; }

        public string? externalId { get; set; }

        public DateTime? createdAt { get; set; }

        public ShopifyGraphQLMetafield? pricingContract { get; set; }

        public ShopifyGraphQLMetafield? templateName { get; set; }
    }

    public class ShopifyGraphQLCompanyQueryResultData
    {
        public ShopifyGraphQLResultPage<ShopifyGraphQLCompany>? companies { get; set; }
    }

    public class ShopifyGraphQLCompanyLocation
    {
        public string? id { get; set; }

        public string? name { get; set; }

        public string? externalId { get; set; }

        public DateTime? createdAt { get; set; }

        public ShopifyGraphQLCompany? company { get; set; }
    }

    public class ShopifyGraphQLCompanyLocationQueryResultData
    {
        public ShopifyGraphQLResultPage<ShopifyGraphQLCompanyLocation>? companyLocations { get; set; }
    }
}

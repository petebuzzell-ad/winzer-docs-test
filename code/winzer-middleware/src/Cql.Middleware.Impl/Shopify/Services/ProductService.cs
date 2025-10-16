using Cql.Middleware.Library.Shopify;
using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Products;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using Dasync.Collections;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Impl.Shopify.Services
{
    public partial class ProductService : BaseService, IShopifyProductService
    {
        public ProductService(ILogger<ProductService> logger, ShopifyGraphQLOptions options) : base(logger, options)
        {
        }
    }
}

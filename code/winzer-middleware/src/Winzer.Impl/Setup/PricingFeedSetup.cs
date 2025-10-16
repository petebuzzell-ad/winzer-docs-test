using Winzer.Impl.Salsify;
using Winzer.Library;
using Winzer.Library.Salsify;
using Cql.Middleware.Impl.Shopify;
using Cql.Middleware.Impl.Shopify.Services;
using Cql.Middleware.Library.Shopify.Products;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Cql.Middleware.Library.Shopify.Company;
using Cql.Middleware.Library.Shopify.Catalog;
using Cql.Middleware.Library.Shopify.PriceList;
using Cql.Middleware.Library.Shopify.Order;
using Cql.Middleware.Impl.Util;
using Cql.Middleware.Library.Util;

namespace Winzer.Impl.Setup
{
    public static class PricingFeedSetup
    {
        public static void ConfigurePricingFeed(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IShopifyProductMerger>(new ShopifyProductMerger(configuration
                    .GetSection("OracleToShopifyVariantFieldMapping")
                    .GetChildren()
                    .Select(x => x.Value)
                    .ToList().Concat(
                        configuration
                            .GetSection("OracleToShopifyProductFieldMapping")
                            .GetChildren()
                            .Select(x => x.Value)
                    )
            ));
            services.AddSingleton<PricingImportOptions>(configuration.GetSection("PricingImport").Get<PricingImportOptions>());
            services.AddSingleton<IShopifyProductService, ProductService>();
            services.AddSingleton<IPricingFeedService, PricingFeedService>();
            services.AddSingleton<ICompanyService, CompanyService>();
            services.AddSingleton<ICatalogService, CatalogService>();
            services.AddSingleton<IPriceListService, PriceListService>();
            services.AddTransient<IFileService, SftpService>();
            services.AddSingleton<SftpOptions>(configuration.GetSection("Sftp").Get<SftpOptions>());
        }
    }
}

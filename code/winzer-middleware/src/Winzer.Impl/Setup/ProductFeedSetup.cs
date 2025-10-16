using Winzer.Impl.Salsify;
using Winzer.Impl.Oracle;
using Winzer.Library;
using Winzer.Library.Salsify;
using Cql.Middleware.Impl.Shopify;
using Cql.Middleware.Impl.Shopify.Services;
using Cql.Middleware.Library.Shopify.Products;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Winzer.Library.Oracle;
using Cql.Middleware.Impl.Util;
using Cql.Middleware.Library.Util;
using Cql.Middleware.Library.Shopify.Catalog;
using Cql.Middleware.Library.Shopify.PriceList;

namespace Winzer.Impl.Setup
{
    public static class ProductFeedSetup
    {
        public static void ConfigureProductFeed(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ProductImportOptions>(configuration.GetSection("ProductImport").Get<ProductImportOptions>());

            services.AddSingleton<OracleProductTransmogrifierConfiguration>(new OracleProductTransmogrifierConfiguration()
            {
                VariantMetafieldMapping = configuration
                    .GetSection("OracleToShopifyVariantFieldMapping")
                    .GetChildren()
                    .ToDictionary(x => x.Key, x => x.Value),
                ProductMetafieldMapping = configuration
                    .GetSection("OracleToShopifyProductFieldMapping")
                    .GetChildren()
                    .ToDictionary(x => x.Key, x => x.Value),
            });

            services.AddSingleton<ISalsifyProductTransmogrifier, SalsifyProductTransmogrifier>();
            services.AddSingleton<IOracleProductTransmogrifier, OracleProductTransmogrifier>();
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
            services.AddSingleton<IShopifyProductService, ProductService>();
            services.AddTransient<IFileService, SftpService>();
            services.AddSingleton<SftpOptions>(configuration.GetSection("Sftp").Get<SftpOptions>());
            services.AddSingleton<IProductFeedService, ProductFeedService>();
            services.AddSingleton<ICatalogService, CatalogService>();
            services.AddSingleton<IPriceListService, PriceListService>();

        }
    }
}

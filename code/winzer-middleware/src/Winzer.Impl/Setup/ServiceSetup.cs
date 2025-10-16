using Cql.Middleware.Impl.Shopify.Services;
using Cql.Middleware.Library.Shopify.Collection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Winzer.Core.Services;

namespace Winzer.Impl.Setup
{
    public static class ServiceSetup
    {
        public static void ConfigureWinzerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IProductMapService, ProductMapService>();
            services.AddSingleton<ITemplatePricingService, TemplatePricingService>();
            services.AddSingleton<IBulkPricingService, BulkPricingService>();
            services.AddSingleton<IVariantMapService, VariantMapService>();
            services.AddSingleton<ICollectionService, CollectionService>();
            services.AddSingleton<IContractPricingService, ContractPricingService>();
            services.AddSingleton<ILastPurchasePricingService, LastPurchasePricingService>();
        }
    }
}

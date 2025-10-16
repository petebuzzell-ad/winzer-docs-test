using Winzer.Impl.Salsify;
using Winzer.Library;
using Winzer.Library.Salsify;
using Cql.Middleware.Impl.Shopify.Services;
using Cql.Middleware.Library.Shopify.Inventory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Winzer.Impl.Setup
{
    public static class InventoryFeedSetup
    {
        public static void ConfigureInventoryFeed(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<SalsifyAPIServiceOptions>(configuration.GetSection("Salsify").Get<SalsifyAPIServiceOptions>());
            services.AddSingleton<ISalsifyAPIService, SalsifyAPIService>();
            services.AddSingleton<InventoryOptions>(configuration.GetSection("Inventory").Get<InventoryOptions>());
            services.AddTransient<IInventoryService, InventoryService>();
            services.AddTransient<IInventoryFeedService, InventoryFeedService>();
            services.AddSingleton<ISalsifyExportParser, SalsifyExportParser>();
        }
    }
}

using Winzer.Impl.Crm;
using Winzer.Library;
using Winzer.Library.Crm;
using Cql.Middleware.Impl.Shopify.Services;
using Cql.Middleware.Library.Shopify.Order;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Winzer.Impl.Setup
{
    public static class OrderExportSetup
    {
        public static void ConfigureOrderExport(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<WinzerCrmOptions>(configuration.GetSection("WinzerCrm").Get<WinzerCrmOptions>());
            services.AddTransient<IWinzerCrmService, WinzerCrmService>();

            services.AddSingleton<OrderOptions>(configuration.GetSection("Order").Get<OrderOptions>());
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IOrderExportService, OrderExportService>();
        }
    }
}

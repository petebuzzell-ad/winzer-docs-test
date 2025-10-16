using Winzer.Library;
using Cql.Middleware.Impl.Shopify.Services;
using Cql.Middleware.Library.Shopify.Shipping;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Winzer.Impl.Setup
{
    public static class FulfillmentFeedSetup
    {
        public static void ConfigureFulfillmentFeed(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<FulfillmentOptions>(configuration.GetSection("Fulfillment").Get<FulfillmentOptions>());
            services.AddTransient<IFulfillmentService, FulfillmentService>();
            services.AddTransient<IFulfillmentFeedService, FulfillmentFeedService>();
        }
    }
}

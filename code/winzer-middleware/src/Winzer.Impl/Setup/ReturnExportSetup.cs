using Winzer.Library;
using Winzer.Library.Loop;
using Winzer.Impl.Loop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Winzer.Impl.Setup
{
    public static class ReturnExportSetup
    {
        public static void ConfigureReturnFeed(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<LoopReturnOptions>(configuration.GetSection("Return").Get<LoopReturnOptions>());
            services.AddTransient<IReturnService, ReturnService>();

            services.AddTransient<IReturnExportService, ReturnExportService>();
        }
    }
}

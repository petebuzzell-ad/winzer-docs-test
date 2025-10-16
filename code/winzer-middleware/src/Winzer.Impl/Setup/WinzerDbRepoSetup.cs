using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Winzer.Core.Repo;
using Winzer.Repo;

namespace Winzer.Impl.Setup
{
    public static class WinzerDbRepoSetup
    {
        public static void ConfigureDbRepos(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RepoOptions>(x => x.WinzerDBConnectionString = configuration.GetConnectionString("WinzerDB"));

            services.AddSingleton<IProductMapRepo, ProductMapRepo>();
            services.AddSingleton<ITemplatePricingRepo, TemplatePricingRepo>();
            services.AddSingleton<IBulkPricingRepo, BulkPricingRepo>();
            services.AddSingleton<IVariantMapRepo, VariantMapRepo>();
            services.AddSingleton<ILastPurchasePricingRepo, LastPurchasePricingRepo>();
            services.AddSingleton<IContractPricingRepo, ContractPricingRepo>();
        }
    }
}

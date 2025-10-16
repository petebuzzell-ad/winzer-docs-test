using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Winzer.Impl.Setup;

namespace Winzer.ShopifyMiddleware.AWS;

// [LambdaStartup]
// Modeled after Amazon.Lambda.Annotations configuration, but that library is in pre-release and not usable yet.
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Startup>()
            .AddEnvironmentVariables()
            .Build();
    }

    /// <summary>
    /// Services for Lambda functions can be registered in the services dependency injection container in this method.
    /// *Services injected for the constructor have the lifetime of the Lambda compute container*
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        var loggerOptions = new LambdaLoggerOptions(Configuration);
        services.AddLogging(builder =>
        {
            builder.AddLambdaLogger(loggerOptions);
        });

        services.ConfigureGraphQL(Configuration);
        services.ConfigureFileService(Configuration);
        services.ConfigureInventoryFeed(Configuration);
        services.ConfigureOrderExport(Configuration);
        services.ConfigureFulfillmentFeed(Configuration);
        services.ConfigureReturnFeed(Configuration);
        services.ConfigureSNS(Configuration);
        services.ConfigureDbRepos(Configuration);
        services.ConfigureWinzerServices(Configuration);
        services.Configure<BaseHandlerOptions>((options) => {
            options.SnsTopicArn = Configuration.GetValue<String>("SnsTopicArn");
            options.WebteamSnsTopicArn = Configuration.GetValue<String>("WebteamSnsTopicArn");
        });
        services.AddSingleton<BaseHandlerOptions>(new BaseHandlerOptions
        {
            SnsTopicArn = Configuration.GetValue<String>("SnsTopicArn"),
            WebteamSnsTopicArn = Configuration.GetValue<String>("WebteamSnsTopicArn")
        });
        ;
    }
}

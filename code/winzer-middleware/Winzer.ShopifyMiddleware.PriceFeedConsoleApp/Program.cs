using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Winzer.Impl.Setup;
using Winzer.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Winzer.Core.Services;

var Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddLog4Net();
});

services.ConfigureGraphQL(Configuration);
services.ConfigureDbRepos(Configuration);
services.ConfigureFileService(Configuration);
services.ConfigurePricingFeed(Configuration);
services.ConfigureSNS(Configuration);
services.ConfigureWinzerServices(Configuration); 

var provider = services.BuildServiceProvider(); 


var productMapService = provider.GetRequiredService<IProductMapService>();
var entryPoint = provider.GetRequiredService<IPricingFeedService>();
var logger = provider.GetRequiredService<ILogger<Program>>();
var sns = provider.GetRequiredService<IAmazonSimpleNotificationService>();
var snsTopic = Configuration.GetValue<string>("SnsTopicArn");

var result = false;
try
{

    logger.LogInformation("Starting Pricing Feed Import");
    result = await entryPoint.ImportPricingData();
    //await entryPoint.DeleteAllTheCatalogs();
    //await entryPoint.AddEverybodyToMillionCatalog();
}
catch (Exception ex)
{
    var message = "Uncaught exception while running the ImportUpdatedPricing job";
    logger.LogError(ex, message);
    result = false;
}

if (result)
{
    logger.LogInformation("Successfully Finished Pricing Feed Import");
}
else
{
    logger.LogError("One or more errors occured during Pricing Feed Import.  Check the logs for details.");
    //var response = await sns.PublishAsync(new PublishRequest(snsTopic, $"One or more errors occured during Pricing Feed Import.  Check the logs for details."));
    //logger.LogInformation("SNS Publish MessageId: {0}", response.MessageId);
}

return result ? 0 : 1;

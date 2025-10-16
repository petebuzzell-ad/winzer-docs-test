using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Winzer.ShopifyMiddleware.AWS;

public abstract class BaseHandler
{
    protected readonly ServiceProvider? ServiceProvider;
    protected readonly IAmazonSimpleNotificationService? Sns;
    protected readonly BaseHandlerOptions BaseOptions;
    private readonly ILogger _logger;
    private readonly string _snsTopicArn;

    protected BaseHandler(ILogger logger, IAmazonSimpleNotificationService _sns, BaseHandlerOptions _options)
    {
        _logger = logger;
        Sns = _sns;
        BaseOptions = _options;
        _snsTopicArn = String.IsNullOrEmpty(BaseOptions.WebteamSnsTopicArn) ? BaseOptions.SnsTopicArn : BaseOptions.WebteamSnsTopicArn;
    }

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    protected BaseHandler(bool _buildServiceProvider)
    {
        var services = new ServiceCollection();
        new Startup().ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        _logger = ServiceProvider.GetRequiredService<ILogger<BaseHandler>>();
        Sns = ServiceProvider.GetService<IAmazonSimpleNotificationService>();
        BaseOptions = ServiceProvider.GetRequiredService<BaseHandlerOptions>();
    }

    protected virtual async Task AlertFatal(Exception? ex, String message)
    {
        _logger.LogCritical(ex, message);
        if(Sns == null || String.IsNullOrEmpty(BaseOptions.SnsTopicArn))
        {
            return;
        }
        var response = await Sns.PublishAsync(new PublishRequest(BaseOptions.SnsTopicArn, $"{message} - {ex?.ToString()}"));
        _logger.LogInformation("SNS Publish MessageId: {0}", response.MessageId);
    }

    protected virtual async Task AlertCustomer(String message)
    {
        if (Sns == null || string.IsNullOrWhiteSpace(_snsTopicArn))
        {
            _logger.LogWarning("SNS Alert skipped because it is not configured.");
            return;
        }

        var response = await Sns.PublishAsync(new PublishRequest(_snsTopicArn, message));
        _logger.LogInformation("SNS Publish MessageId: {0}", response.MessageId);
    }
}

public class BaseHandlerOptions
{
    public String SnsTopicArn { get; set; } = String.Empty;

    public String WebteamSnsTopicArn { get; set; } = String.Empty;
}

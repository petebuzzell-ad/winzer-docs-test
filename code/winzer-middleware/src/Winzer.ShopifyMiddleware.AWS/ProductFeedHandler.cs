using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Winzer.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Winzer.ShopifyMiddleware.AWS;

public class ProductFeedHandler : BaseHandler
{
    private readonly IProductFeedService _productFeedService;
    private readonly ILogger<ProductFeedHandler> _logger;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public ProductFeedHandler()
    : base(true)
    {
        _productFeedService = ServiceProvider!.GetRequiredService<IProductFeedService>();
        _logger = ServiceProvider!.GetRequiredService<ILogger<ProductFeedHandler>>();
    }

    /// <summary>
    /// Constructor for Dependency Injection.
    /// We'll be able to use this with Amazon.Lambda.Annotations, but that library is in pre-release and not usable yet.
    /// It'll be real nice when the constructor can look like this.
    /// </summary>
    public ProductFeedHandler(IProductFeedService productFeedService, ILogger<ProductFeedHandler> logger, IAmazonSimpleNotificationService sns, BaseHandlerOptions options)
    : base(logger, sns, options)
    {
        _productFeedService = productFeedService;
        _logger = logger;
    }

    public async Task<bool> Handler(ILambdaContext _context)
    {
        _logger.LogInformation("Starting Product Feed Import");
        var result = false;
        try
        {
            result = await _productFeedService.ImportEverything();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Uncaught exception while running the ImportUpdatedProducts job");
            result = false;
        }
        if (result)
        {
            _logger.LogInformation("Successfully Finished Product Feed Import");
        }
        else
        {
            _logger.LogError("One or more errors occured during Product Feed Import.  Check the logs for details.");
        }
        return result;
    }
}

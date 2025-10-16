using System.Diagnostics;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Winzer.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Winzer.ShopifyMiddleware.AWS;

public class EcommerceInventoryFeedHandler : BaseHandler
{
    private readonly IInventoryFeedService _inventoryFeedService;
    private readonly ILogger<EcommerceInventoryFeedHandler> _logger;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public EcommerceInventoryFeedHandler()
    : base(true)
    {
        _inventoryFeedService = ServiceProvider!.GetRequiredService<IInventoryFeedService>();
        _logger = ServiceProvider!.GetRequiredService<ILogger<EcommerceInventoryFeedHandler>>();
    }

    /// <summary>
    /// Constructor for Dependency Injection.
    /// We'll be able to use this with Amazon.Lambda.Annotations, but that library is in pre-release and not usable yet.
    /// It'll be real nice when the constructor can look like this.
    /// </summary>
    public EcommerceInventoryFeedHandler(IInventoryFeedService inventoryFeedService, ILogger<EcommerceInventoryFeedHandler> logger, IAmazonSimpleNotificationService sns, BaseHandlerOptions options)
    : base(logger, sns, options)
    {
        _inventoryFeedService = inventoryFeedService;
        _logger = logger;
    }

    public async Task<InventoryFeedResponse> Handler(InventoryFeedRequest request, ILambdaContext _context)
    {
        _logger.LogInformation("Starting Ecommerce Inventory Feed Import");
        var result = new List<InventoryFeedResult>();
        var fileList = request.FileNames;

        if (String.IsNullOrEmpty(request.LocationId))
        {
            throw new ArgumentException("You must provide a Location ID for the main Ecommerce Inventory");
        }

        if (!fileList.Any() && !String.IsNullOrEmpty(request.Path) && !String.IsNullOrEmpty(request.Pattern))
        {
            _logger.LogInformation("Searching {path} for files matching {pattern}", request.Path, request.Pattern);
            fileList = await _inventoryFeedService.FindInventoryFiles(request.Path, request.Pattern);
        }
        if (fileList.Count() > 1)
            _logger.LogInformation("Multiple files found.  Running the first one.");
        if (!fileList.Any())
            _logger.LogInformation("No files found");
        foreach (var fileName in fileList.Take(1))
        {
            var started = DateTimeOffset.Now;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Processing {fileName}", fileName);
                var feedResult = await _inventoryFeedService.ImportEcommerceInventoryFeed(fileName, request.LocationId, request.Dryrun);
                result.Add(new InventoryFeedResult
                {
                    FileName = fileName,
                    Message = feedResult
                });
            }
            catch(Exception ex)
            {
                await AlertFatal(ex, $"Fatal Error while processing {fileName}");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation("Ecommerce Inventory Feed Import Started at: {started} duration: {elapsed}", started, stopwatch.Elapsed);
            }
        }

        _logger.LogInformation("Finished Ecommerce Inventory Feed Import");

        return new InventoryFeedResponse
        {
            Results = result
        };
    }
}

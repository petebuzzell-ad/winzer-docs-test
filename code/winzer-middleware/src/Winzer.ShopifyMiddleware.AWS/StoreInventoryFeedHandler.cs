using System.Diagnostics;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Winzer.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Winzer.ShopifyMiddleware.AWS;

public class StoreInventoryFeedHandler : BaseHandler
{
    private readonly IInventoryFeedService _inventoryFeedService;
    private readonly ILogger<StoreInventoryFeedHandler> _logger;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public StoreInventoryFeedHandler()
    : base(true)
    {
        _inventoryFeedService = ServiceProvider!.GetRequiredService<IInventoryFeedService>();
        _logger = ServiceProvider!.GetRequiredService<ILogger<StoreInventoryFeedHandler>>();
    }

    /// <summary>
    /// Constructor for Dependency Injection.
    /// We'll be able to use this with Amazon.Lambda.Annotations, but that library is in pre-release and not usable yet.
    /// It'll be real nice when the constructor can look like this.
    /// </summary>
    public StoreInventoryFeedHandler(IInventoryFeedService inventoryFeedService, ILogger<StoreInventoryFeedHandler> logger, IAmazonSimpleNotificationService sns, BaseHandlerOptions options)
    : base(logger, sns, options)
    {
        _inventoryFeedService = inventoryFeedService;
        _logger = logger;
    }

    public async Task<InventoryFeedResponse> Handler(InventoryFeedRequest request, ILambdaContext _context)
    {
        _logger.LogInformation("Starting Inventory Feed Import");
        var results = new List<InventoryFeedResult>();
        var fileList = request.FileNames;

        if (!fileList.Any() && !String.IsNullOrEmpty(request.Path) && !String.IsNullOrEmpty(request.Pattern))
        {
            _logger.LogInformation("Searching {path} for files matching {pattern}", request.Path, request.Pattern);
            fileList = await _inventoryFeedService.FindInventoryFiles(request.Path, request.Pattern);
        }

        foreach (var fileName in fileList)
        {
            var started = DateTimeOffset.Now;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Processing {fileName}", fileName);
                var result = await _inventoryFeedService.ImportStoreInventoryFeed(fileName, request.Dryrun);
                results.Add(new InventoryFeedResult
                {
                    FileName = fileName,
                    Message = result
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
                _logger.LogInformation("Inventory Feed Import Started at: {started} duration: {elapsed}", started, stopwatch.Elapsed);
            }
        }
        _logger.LogInformation("Finished Inventory Feed Import");

        return new InventoryFeedResponse
        {
            Results = results
        };
    }
}

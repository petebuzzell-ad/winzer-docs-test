using System.Diagnostics;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Winzer.Library;
using Winzer.Library.Salsify;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Winzer.ShopifyMiddleware.AWS;

public class SalsifyInventoryFeedHandler : BaseHandler
{
    private readonly IInventoryFeedService _inventoryFeedService;
    private readonly ISalsifyAPIService _salsifyApi;
    private readonly ILogger<EcommerceInventoryFeedHandler> _logger;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public SalsifyInventoryFeedHandler()
    : base(true)
    {
        _inventoryFeedService = ServiceProvider!.GetRequiredService<IInventoryFeedService>();
        _salsifyApi = ServiceProvider!.GetRequiredService<ISalsifyAPIService>();
        _logger = ServiceProvider!.GetRequiredService<ILogger<EcommerceInventoryFeedHandler>>();
    }

    /// <summary>
    /// Constructor for Dependency Injection.
    /// We'll be able to use this with Amazon.Lambda.Annotations, but that library is in pre-release and not usable yet.
    /// It'll be real nice when the constructor can look like this.
    /// </summary>
    public SalsifyInventoryFeedHandler(ISalsifyAPIService salsifyApi, IInventoryFeedService inventoryFeedService, ILogger<EcommerceInventoryFeedHandler> logger, IAmazonSimpleNotificationService sns, BaseHandlerOptions options)
    : base(logger, sns, options)
    {
        _inventoryFeedService = inventoryFeedService;
        _salsifyApi = salsifyApi;
        _logger = logger;
    }

    public async Task Handler(InventoryFeedRequest request, ILambdaContext _context)
    {
        _logger.LogInformation("Starting Salsify Inventory Feed Import");
        var fileList = request.FileNames;

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
                var result = await _inventoryFeedService.ImportSalsifyInventoryFeed(fileName, request.Dryrun);
                if (result.HasErrors)
                {
                    await AlertFatal(null, $"At least one error occurred while processing {fileName}.  Check the logs for details.");
                }

                if (result.ReactivationAttempts.Any())
                {
                    // Send SNS to web team...
                    StringBuilder message = new StringBuilder();
                    message.AppendLine("The following products were previously marked as EOL, but have been re-activated or brought back into inventory.  Please review the products below and manually set the 'Send To Shopify' property to 'Yes' in Salsify if these products should be available for sale on the website.");
                    message.AppendLine();
                    foreach ( var sku in result.ReactivationAttempts)
                    {
                        message.AppendLine(sku);
                    }
                    await AlertCustomer(message.ToString());
                }
            }
            catch (Exception ex)
            {
                await AlertFatal(ex, $"Fatal Error while processing {fileName}");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation("Salsify Inventory Feed Import Started at: {started} duration: {elapsed}", started, stopwatch.Elapsed);
            }
        }

        _logger.LogInformation("Finished Salsify Inventory Feed Import");
    }
}

using System.Diagnostics;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Winzer.Library;
using Winzer.Library.Error;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Winzer.ShopifyMiddleware.AWS;

public class FulfillmentFeedHandler : BaseHandler
{
    private readonly ILogger<FulfillmentFeedHandler> _logger;
    private readonly BaseHandlerOptions _options;
    private readonly IFulfillmentFeedService _service;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public FulfillmentFeedHandler()
    : base(true)
    {
        _logger = ServiceProvider!.GetRequiredService<ILogger<FulfillmentFeedHandler>>();
        _options = ServiceProvider!.GetRequiredService<BaseHandlerOptions>();
        _service = ServiceProvider!.GetRequiredService<IFulfillmentFeedService>();
    }

    /// <summary>
    /// Constructor for Dependency Injection.
    /// We'll be able to use this with Amazon.Lambda.Annotations, but that library is in pre-release and not usable yet.
    /// It'll be real nice when the constructor can look like this.
    /// </summary>
    public FulfillmentFeedHandler(ILogger<FulfillmentFeedHandler> logger, IAmazonSimpleNotificationService sns, BaseHandlerOptions options, IFulfillmentFeedService service)
    : base(logger, sns, options)
    {
        _logger = logger;
        _options = options;
        _service = service;
    }

    public async Task<FulfillmentFeedResponse> Handler(FulfillmentFeedRequest request, ILambdaContext _context)
    {
        _logger.LogInformation("Starting Fulfillment Import");
        var results = new List<FulfillmentFeedResult>();
        var fileList = request.FileNames;

        if (!fileList.Any() && !String.IsNullOrEmpty(request.Path) && !String.IsNullOrEmpty(request.Pattern))
        {
            _logger.LogInformation("Searching {path} for files matching {pattern}", request.Path, request.Pattern);
            fileList = await _service.FindFulfillmentFiles(request.Path, request.Pattern);
        }

        foreach (var fileName in fileList)
        {
            var started = DateTimeOffset.Now;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Processing {fileName}", fileName);
                var result = await _service.ImportFulfillmentFeed(fileName, request);
                results.Add(new FulfillmentFeedResult
                {
                    FileName = fileName,
                    Message = result
                });
            }
            catch(FileTooBigException ex)
            {
                _logger.LogError(ex, "File too big for processing");
                await AlertCustomer($"File too big for processing: {ex.Message}");
            }
            catch(Exception ex)
            {
                await AlertFatal(ex, $"Fatal Error while processing {fileName}");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation("Fulfillment Feed Import Started at: {started} duration: {elapsed}", started, stopwatch.Elapsed);
            }
        }

        _logger.LogInformation("Finished Fulfillment Export");

        return new FulfillmentFeedResponse
        {
            Results = results
        };
    }
}

public class FulfillmentFeedRequest : TestContext
{
    public IEnumerable<String> FileNames { get; set; } = new List<String>();
    public String? Path { get; set; }
    public String? Pattern { get; set; }
}
public class FulfillmentFeedResult
{
    public string FileName { get; set; } = String.Empty;
    public string Message { get; set; } = String.Empty;
}

public class FulfillmentFeedResponse
{
    public IEnumerable<FulfillmentFeedResult> Results { get; set; } = Enumerable.Empty<FulfillmentFeedResult>();
}

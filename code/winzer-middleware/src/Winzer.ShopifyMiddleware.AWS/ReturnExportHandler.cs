using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Winzer.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Winzer.ShopifyMiddleware.AWS;

public class ReturnExportHandler : BaseHandler
{
    private readonly ILogger<ReturnExportHandler> _logger;
    private readonly BaseHandlerOptions _options;
    private readonly IReturnExportService _service;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public ReturnExportHandler()
    : base(true)
    {
        _logger = ServiceProvider!.GetRequiredService<ILogger<ReturnExportHandler>>();
        _options = ServiceProvider!.GetRequiredService<BaseHandlerOptions>();
        _service = ServiceProvider!.GetRequiredService<IReturnExportService>();
    }

    /// <summary>
    /// Constructor for Dependency Injection.
    /// We'll be able to use this with Amazon.Lambda.Annotations, but that library is in pre-release and not usable yet.
    /// It'll be real nice when the constructor can look like this.
    /// </summary>
    public ReturnExportHandler(ILogger<ReturnExportHandler> logger, IAmazonSimpleNotificationService sns, BaseHandlerOptions options, IReturnExportService service)
    : base(logger, sns, options)
    {
        _logger = logger;
        _options = options;
        _service = service;
    }

    public async Task<String> Handler(ReturnExportRequest request, ILambdaContext _context)
    {
        _logger.LogInformation("Starting Return Export");
        String result;
        try
        {
            result = await _service.ExportReturns(request.Since, request.To, request);
        }
        catch (Exception ex)
        {
            await AlertFatal(ex, "Fatal Error while exporting returns");
            throw;
        }

        _logger.LogInformation("Finished Return Export");

        return result;
    }
}

public class ReturnExportRequest : TestContext
{
    public DateTime Since { get; set; } = DateTime.Now.AddDays(-7);
    public DateTime To { get; set; } = DateTime.Now;
}

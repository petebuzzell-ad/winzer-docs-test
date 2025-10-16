using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Winzer.Library;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Winzer.ShopifyMiddleware.AWS;

public class OrderExportHandler : BaseHandler
{
    private readonly ILogger<OrderExportHandler> _logger;
    private readonly BaseHandlerOptions _options;
    private readonly IOrderExportService _service;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public OrderExportHandler()
    : base(true)
    {
        _logger = ServiceProvider!.GetRequiredService<ILogger<OrderExportHandler>>();
        _options = ServiceProvider!.GetRequiredService<BaseHandlerOptions>();
        _service = ServiceProvider!.GetRequiredService<IOrderExportService>();
    }

    /// <summary>
    /// Constructor for Dependency Injection.
    /// We'll be able to use this with Amazon.Lambda.Annotations, but that library is in pre-release and not usable yet.
    /// It'll be real nice when the constructor can look like this.
    /// </summary>
    public OrderExportHandler(ILogger<OrderExportHandler> logger, IAmazonSimpleNotificationService sns, BaseHandlerOptions options, IOrderExportService service)
    : base(logger, sns, options)
    {
        _logger = logger;
        _options = options;
        _service = service;
    }

    public async Task<OrderExportResponse> Handler(OrderExportRequest request, ILambdaContext _context)
    {
        _logger.LogInformation("Starting Order Export");
        OrderExportResponse result;
        try
        {
            result = await _service.ExportOrders(request.Since, request);
        }
        catch (Exception ex)
        {
            await AlertFatal(ex, "Fatal Error while exporting orders");
            throw;
        }

        _logger.LogInformation("Finished Order Export");

        return result;
    }
}

public class OrderExportRequest : TestContext
{
    public DateTime Since { get; set; } = DateTime.Now.AddDays(-30);
}

using Microsoft.Extensions.Logging;
using Winzer.Library.Loop;
using RestSharp;

namespace Winzer.Impl.Loop;

public class ReturnService : IReturnService
{

    private readonly ILogger<ReturnService> _logger;
    private readonly LoopReturnOptions _options;
    public RestClient? _restClient { get; set; }

    public ReturnService(ILogger<ReturnService> logger, LoopReturnOptions options)
    {
        _logger = logger;
        _options = options;
        if(!String.IsNullOrEmpty(_options.EndpointUrl) && !String.IsNullOrEmpty(_options.AuthorizationHeader))
            _restClient = new RestClient(new Uri(_options.EndpointUrl))
                .AddDefaultHeader("X-Authorization", _options.AuthorizationHeader);
    }

    public async Task<IEnumerable<LoopReturn>> LookupReturns(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        if (_restClient == null){
            _logger.LogWarning("Loop API is not configured");
            return Enumerable.Empty<LoopReturn>();
        }
        var request = new RestRequest(_options.EndpointUrl)
            .AddQueryParameter("from", from.ToString("yyyy-MM-dd HH:mm:ss"))
            .AddQueryParameter("to", to.ToString("yyyy-MM-dd HH:mm:ss"))
            .AddQueryParameter("filter", "updated_at")
            .AddQueryParameter("state", "closed");
        try
        {
            var response = await _restClient.GetAsync<IEnumerable<LoopReturn>>(request, cancellationToken);
            return response ?? Enumerable.Empty<LoopReturn>();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error: Looking up orders from Loop");
        }

        return Enumerable.Empty<LoopReturn>();
    }
}

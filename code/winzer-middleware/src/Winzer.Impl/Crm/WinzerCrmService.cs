using Microsoft.Extensions.Logging;
using Winzer.Library.Crm;
using RestSharp;

namespace Winzer.Impl.Crm;

public class WinzerCrmService : IWinzerCrmService
{

    private readonly ILogger<WinzerCrmService> _logger;
    private readonly WinzerCrmOptions _options;
    public RestClient? _restClient { get; set; }

    public WinzerCrmService(ILogger<WinzerCrmService> logger, WinzerCrmOptions options)
    {
        _logger = logger;
        _options = options;
        if(!String.IsNullOrEmpty(_options.EndpointUrl) && !String.IsNullOrEmpty(_options.AuthorizationHeader))
            _restClient = new RestClient(new Uri(_options.EndpointUrl))
                .AddDefaultHeader("x-api-key", _options.AuthorizationHeader);
    }

    public async Task<String?> LookupCustomerId(String? email, CancellationToken cancellationToken = default)
    {
        if (_restClient == null || String.IsNullOrEmpty(email)){
            return null;
        }
        var request = new RestRequest(_options.EndpointUrl)
            .AddJsonBody(
                new {
                    email
                }
            );
        try
        {
            var response = await _restClient.PostAsync<CrmResponse>(request, cancellationToken);
            return response?.customer_id;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error: CRM Customer Lookup failed for customer: {email} ", email);
        }

        return null;
    }
}

public class CrmResponse {
    public string subscriber_key { get; set; } = String.Empty;
    public string customer_id { get; set; } = String.Empty;
}

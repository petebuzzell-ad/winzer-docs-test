namespace Winzer.Library.Crm;

public interface IWinzerCrmService
{
    Task<String?> LookupCustomerId(String? email, CancellationToken cancellationToken = default);
}

public class WinzerCrmOptions
{
    public string EndpointUrl { get; set; } = String.Empty;
    public string AuthorizationHeader { get; set; } = String.Empty;
}

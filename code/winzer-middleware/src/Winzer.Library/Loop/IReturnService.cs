using Cql.Middleware.Library.Shopify.Order;

namespace Winzer.Library.Loop;

public interface IReturnService
{
    Task<IEnumerable<LoopReturn>> LookupReturns(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}

public class LoopReturnOptions : OrderOptions
{
    public string EndpointUrl { get; set; } = String.Empty;
    public string AuthorizationHeader { get; set; } = String.Empty;
    public string ReturnMetafieldKey { get; set; } = "returns_metadata";
}

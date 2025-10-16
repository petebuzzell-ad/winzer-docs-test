namespace Winzer.Library;

public interface IOrderExportService
{
    public Task<OrderExportResponse> ExportOrders(DateTime since, TestContext testContext, CancellationToken cancellationToken = default);
}

public class OrderExportResponse
{
    public int FailedOrderCount { get; set; }
    public string Message { get; set; } = String.Empty;
}

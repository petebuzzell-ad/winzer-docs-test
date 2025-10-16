namespace Winzer.Library
{
    public interface IReturnExportService
    {
        public Task<String> ExportReturns(DateTime since, DateTime to, TestContext testContext, CancellationToken cancellationToken = default);
    }
}

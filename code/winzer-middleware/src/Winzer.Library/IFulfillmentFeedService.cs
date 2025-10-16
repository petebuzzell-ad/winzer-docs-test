namespace Winzer.Library
{
    public interface IFulfillmentFeedService
    {
        public Task<IEnumerable<String>> FindFulfillmentFiles(string path, string pattern, CancellationToken cancellationToken = default);
        public Task<String> ImportFulfillmentFeed(string fileName, TestContext testContext, CancellationToken cancellationToken = default);
    }
}

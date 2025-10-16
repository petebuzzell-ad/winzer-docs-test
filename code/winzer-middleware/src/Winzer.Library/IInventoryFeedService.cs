namespace Winzer.Library
{
    public interface IInventoryFeedService
    {
        public Task<String> ImportStoreInventoryFeed(string fileName, bool dryrun, CancellationToken cancellationToken = default);
        public Task<String> ImportEcommerceInventoryFeed(string fileName, string locationId, bool dryrun, CancellationToken cancellationToken = default);
        public Task<SalsifyInventoryImportResult> ImportSalsifyInventoryFeed(string fileName, bool dryrun, CancellationToken cancellationToken = default);
        public Task<IEnumerable<String>> FindInventoryFiles(string path, string pattern, CancellationToken cancellationToken = default);
    }

    public class SalsifyInventoryImportResult
    {
        /// <summary>
        /// Indicates if any errors occurred during the process.  Doesn't necessarily indicate total failure though.
        /// </summary>
        public bool HasErrors { get; set; } = false;

        /// <summary>
        /// A list of SKUs that were previously marked as EOL but then mysteriously reactivated.
        /// Used to send a message to Winzer web team to investigate.
        /// </summary>
        public IList<string> ReactivationAttempts { get; set; } = new List<string>();
    }
}

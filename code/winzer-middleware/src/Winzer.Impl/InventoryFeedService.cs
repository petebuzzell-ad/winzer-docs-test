using System.Text;
using Microsoft.Extensions.Logging;

using Winzer.Library;
using Winzer.Library.Inventory;
using Winzer.Library.Store;
using Winzer.Impl.Inventory;
using Winzer.Impl.Store;
using Cql.Middleware.Library.Shopify.Inventory;
using Cql.Middleware.Library.Util;

using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Collections.Concurrent;
using Winzer.Library.Salsify;

namespace Winzer.Impl
{
    public class InventoryFeedService : IInventoryFeedService
    {
        private readonly ILogger<InventoryFeedService> _logger;
        private readonly IFileService _sftpService;
        private readonly IInventoryService _inventoryService;
        private readonly InventoryOptions _options;
        private readonly ISalsifyAPIService _salsifyApi;
        private readonly ISalsifyExportParser _salsifyExportParser;

        public InventoryFeedService(ISalsifyAPIService salsifyApi, ISalsifyExportParser salsifyExportParser, ILogger<InventoryFeedService> logger, IFileService sftpService, IInventoryService inventoryService, InventoryOptions options)
        {
            _logger = logger;
            _sftpService = sftpService;
            _inventoryService = inventoryService;
            _salsifyExportParser = salsifyExportParser;
            _options = options;
            _salsifyApi = salsifyApi;
        }

        public async Task<IEnumerable<String>> FindInventoryFiles(string path, string pattern, CancellationToken cancellationToken = default)
        {
            Regex rgx = new Regex(pattern);
            var files = await _sftpService.ListDirectoryAsync(path, cancellationToken);
            var results = new List<String>();
            return files.Where((f) => rgx.IsMatch(f)).OrderBy(x => x);
        }

        public async Task<SalsifyInventoryImportResult> ImportSalsifyInventoryFeed(string fileName, bool dryrun, CancellationToken cancellationToken = default)
        {
            var rval = new SalsifyInventoryImportResult();

            _logger.LogInformation($"Getting existing salsify data...");
            var exportRequest = new SalsifyExportRequest()
            {
                Filter = "=list:default",
                Format = "json",
                IncludeAllColumns = false,
                PropertiesToExport = { "ProductID", "salsify:parent_id", "salsify:id", "Send To Shopify", "Inventory Lifecycle Code", "New Product", "In Stock" }
            };
            var exportStream = await _salsifyApi.CreateProductExport(exportRequest);
            var json = await new StreamReader(exportStream).ReadToEndAsync();
            //File.WriteAllText("salsify.json", json);
            var salsifyProducts = _salsifyExportParser.ParseSalsifyJsonExport(json);
            var salsifySkuDictionary = GetLeafNodes(salsifyProducts)
                .ToDictionary(p => p.SalsifyId ?? "");

            _logger.LogInformation("Importing Inventory from {0}", fileName);
            var records = await FetchEcommerceInventoryFeed(fileName, cancellationToken);
            _logger.LogInformation($"Processing {records.Count()} records from inventory file.");

            IList<SalsifyProduct> updateQueue = new List<SalsifyProduct>();
            foreach ( var record in records)
            {
                if (salsifySkuDictionary.ContainsKey(record.Sku))
                {
                    var salsifyProduct = salsifySkuDictionary[record.Sku];
                    var sendToShopify = salsifyProduct.GetPropertyValue<bool>("Send To Shopify");
                    var salsifyInventoryLifecycleCode = salsifyProduct.GetPropertyValueAsString("Inventory Lifecycle Code");
                    var salsifyIsNew = salsifyProduct.GetPropertyValue<bool?>("New Product");
                    var salsifyIsInStock = salsifyProduct.GetPropertyValue<bool?>("In Stock");

                    // Sanity Check: If product is already EOL in salsify and it changes to something else or
                    // the qty goes back above zero, send an alert.
                    if (salsifyInventoryLifecycleCode == "10" && !sendToShopify)
                    {
                        if (record.ProductInventoryLifecycle != "10" || record.Inventory > 0)
                        {
                            rval.ReactivationAttempts.Add(record.Sku);
                        }
                    }

                    bool updateRequired = false;

                    if (salsifyInventoryLifecycleCode != record.ProductInventoryLifecycle)
                    {
                        salsifyProduct["Inventory Lifecycle Code"] = record.ProductInventoryLifecycle;
                        updateRequired = true;
                    }

                    if (salsifyIsNew == null || salsifyIsNew != (record.IsNew == "1"))
                    {
                        salsifyProduct["New Product"] = (record.IsNew == "1");
                        updateRequired = true;
                    }

                    var isInStockNow = record.Inventory > 0;
                    if (salsifyIsInStock != isInStockNow)
                    {
                        salsifyProduct["In Stock"] = isInStockNow;
                        updateRequired = true;
                    }

                    // When product is EOL and inventory drops to zero, set Sent To Shopify to No.
                    if (record.Inventory <= 0 && record.ProductInventoryLifecycle == "10" && sendToShopify)
                    {
                        salsifyProduct["Send To Shopify"] = false;
                        updateRequired = true;
                    }

                    if (updateRequired)
                    {
                        updateQueue.Add(salsifyProduct);
                    }
                }
            }

            if (rval.ReactivationAttempts.Count() > 0)
            {
                _logger.LogInformation($"Found {rval.ReactivationAttempts.Count()} reactivation attempts.");
            }

            _logger.LogInformation($"Updating {updateQueue.Count()} products in Salsify...");
            // Partition the update queue into groups of 100.
            var updateBatches = updateQueue.InSetsOf(100);
            await Parallel.ForEachAsync(updateBatches, async (batch, token) =>
            {
                try
                {
                    await _salsifyApi.UpdateProductsBatch(batch);
                    _logger.LogInformation($"Batch finished.");
                }
                catch ( Exception ex )
                {
                    _logger.LogError(ex, $"Batch update failed.");
                    rval.HasErrors = true;
                }
            });

            _logger.LogInformation($"Finished updating products in Salsify.");

            return rval;
        }


        public async Task<String> ImportStoreInventoryFeed(string fileName, bool dryrun, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Importing Inventory from {0}", fileName);
            var storeIdLookup = await FetchStoreIdLookup(_options.StoreIdLookupFileName, cancellationToken);
            var records = await FetchInventoryFeed(fileName, cancellationToken);

            var activateQueue = new List<InventoryAdjustQuantity>();
            var bulkUpdateQueue = new List<InventoryAdjustQuantity>();
            foreach(var group in records.GroupBy((r) => r.StoreId))
            {
                String locationId;
                if (!storeIdLookup.TryGetValue(group.Key, out locationId!))
                {
                    _logger.LogError("Could not find location Id for {StoreId}", group.Key);
                    continue;
                }
                var inventoryLookup = await FetchInventoryItemLookup(locationId, cancellationToken);
                foreach (var inventoryNew in group)
                {
                    if (inventoryLookup.TryGetValue(inventoryNew.Sku, out IList<InventoryItem>? result) && result.Any())
                    {
                        if (_options.WarnDuplicateSkus && result.First().DuplicateSkuCount > 0) {
                            _logger.LogError("SKU {Sku} has {DuplicateSkuCount} duplicates", inventoryNew.Sku, result.First().DuplicateSkuCount);
                        }
                        foreach (var inventoryItem in result)
                        {
                            if (inventoryItem.InventoryLevel != null && inventoryNew.Inventory != inventoryItem.InventoryLevel.Available)
                            {
                                bulkUpdateQueue.Add(new InventoryAdjustQuantity
                                {
                                    InventoryItemId = inventoryItem.Id,
                                    AvailableDelta = inventoryNew.Inventory - inventoryItem.InventoryLevel.Available,
                                    LocationId = locationId
                                });
                            }
                        }
                    }
                    else if(_options.ActivateInventorySkus)
                    {
                        _logger.LogError("SKU {Sku} is not active for Location {LocationId}", inventoryNew.Sku, locationId);
                        var x = await _inventoryService.LookupInventory(new ShopifyInventoryQuery(_options) {
                            LocationId = locationId,
                            Sku = inventoryNew.Sku
                        }, cancellationToken);
                        if (_options.WarnDuplicateSkus && x.FirstOrDefault()?.DuplicateSkuCount > 0) {
                            _logger.LogError("SKU {Sku} has {DuplicateSkuCount} duplicates", inventoryNew.Sku, x.First().DuplicateSkuCount);
                        }
                        foreach (var inventoryItem in x)
                        {
                            activateQueue.Add(new InventoryAdjustQuantity
                                {
                                    InventoryItemId = inventoryItem.Id,
                                    AvailableDelta = inventoryNew.Inventory,
                                    LocationId = locationId
                                });
                        }
                    }
                }
                _logger.LogInformation("Processed {0} Inventory Records", group.Count());
            }

            var response = "Nothing To Do";
            if (!dryrun)
            {
                var bulkUpdateQueueTask = bulkUpdateQueue.Any() ? _inventoryService.InventoryBulkUpdate(bulkUpdateQueue, cancellationToken) : Task.FromResult(0);
                var activateQueueTask = activateQueue.Any() ? _inventoryService.InventoryActivate(activateQueue, cancellationToken) : Task.FromResult(0);
                await Task.WhenAll(bulkUpdateQueueTask, activateQueueTask);
                _logger.LogInformation("{0} Records Updated", bulkUpdateQueueTask.Result);
                _logger.LogInformation("{0} Records Activated", activateQueueTask.Result);
                response = $"OK - {bulkUpdateQueueTask.Result} Records Updated";
                var success = await ArchiveInventoryFile(fileName);
                if(!success)
                {
                    _logger.LogError("Error Archiving File {fileName}", fileName);
                }
            }
            else
            {
                response = $"DryRun - {records.Count()} Records Updated";
            }

            return response;
        }

        public async Task<String> ImportEcommerceInventoryFeed(string fileName, string locationId, bool dryrun, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Importing Inventory from {0}", fileName);

            var activateQueue = new ConcurrentBag<InventoryAdjustQuantity>();
            var bulkUpdateQueue = new ConcurrentBag<InventoryAdjustQuantity>();
            var inventoryLookup = await FetchInventoryItemLookup(locationId, cancellationToken);
            int recordCount = 0;

            var csvStream = await _sftpService.DownloadFileAsync(fileName, cancellationToken);
            var csvRecords = ParseEcommerceInventoryFeed(csvStream);

            foreach (var inventoryNew in csvRecords)
            {
                if (inventoryLookup.TryGetValue(inventoryNew.Sku, out IList<InventoryItem>? result) && result.Any())
                {
                    if (_options.WarnDuplicateSkus && result.FirstOrDefault()?.DuplicateSkuCount > 0) {
                        _logger.LogError("SKU {Sku} has {DuplicateSkuCount} duplicates", inventoryNew.Sku, result.First().DuplicateSkuCount);
                    }
                    foreach (var inventoryItem in result)
                    {
                        if (inventoryItem.InventoryLevel != null && inventoryNew.Inventory != inventoryItem.InventoryLevel.Available)
                        {
                            bulkUpdateQueue.Add(new InventoryAdjustQuantity
                            {
                                InventoryItemId = inventoryItem.Id,
                                AvailableDelta = inventoryNew.Inventory - inventoryItem.InventoryLevel.Available,
                                LocationId = locationId
                            });
                        }
                    }
                }
                else if(_options.ActivateInventorySkus)
                {
                    _logger.LogError("SKU {Sku} is not active for Location {LocationId}", inventoryNew.Sku, locationId);
                    var x = await _inventoryService.LookupInventory(new ShopifyInventoryQuery(_options) {
                        LocationId = locationId,
                        Sku = inventoryNew.Sku
                    }, cancellationToken);
                    if (_options.WarnDuplicateSkus && x.FirstOrDefault()?.DuplicateSkuCount > 0) {
                        _logger.LogError("SKU {Sku} has {DuplicateSkuCount} duplicates", inventoryNew.Sku, x.First().DuplicateSkuCount);
                    }
                    foreach (var inventoryItem in x)
                    {
                        activateQueue.Add(new InventoryAdjustQuantity
                            {
                                InventoryItemId = inventoryItem.Id,
                                AvailableDelta = inventoryNew.Inventory,
                                LocationId = locationId
                            });
                    }
                }
                recordCount++;
            }
            _logger.LogInformation("Processed {0} Inventory Records", recordCount);

            var response = "Nothing To Do";
            if (!dryrun)
            {
                var bulkUpdateQueueTask = bulkUpdateQueue.Any() ? _inventoryService.InventoryBulkUpdate(bulkUpdateQueue, cancellationToken) : Task.FromResult(0);
                var activateQueueTask = activateQueue.Any() ? _inventoryService.InventoryActivate(activateQueue, cancellationToken) : Task.FromResult(0);
                await Task.WhenAll(bulkUpdateQueueTask, activateQueueTask);
                _logger.LogInformation("{0} Records Updated", bulkUpdateQueueTask.Result);
                _logger.LogInformation("{0} Records Activated", activateQueueTask.Result);
                response = $"OK - {bulkUpdateQueueTask.Result} Records Updated";
                if(_options.ArchiveInventoryFile)
                {
                    var archiveResult = await ArchiveInventoryFile(fileName, cancellationToken);
                    if(!archiveResult)
                        _logger.LogError("Error Archiving File {fileName}", fileName);
                }
                if(_options.CopyInventoryFileForSalsify)
                {
                    var copyResult = await CopyInventoryFileForSalsify(fileName, csvStream, cancellationToken);
                    if(!copyResult)
                        _logger.LogError("Error Copying File {fileName} for Salsify", fileName);
                }
            }
            else
            {
                response = $"DryRun - {recordCount} Records Updated";
            }

            return response;
        }

        private async Task<bool> CopyInventoryFileForSalsify(string filename, byte[] fileData, CancellationToken cancellationToken = default)
        {
            var parts = filename.Split('/').ToList();
            parts[parts.Count() - 1] = _options.SalsifyInventoryFileName;
            var salsifyInventoryFileName = String.Join('/', parts.ToArray());
            _logger.LogInformation("Copying inventory file to {0}", salsifyInventoryFileName);

            await _sftpService.DeleteFileAsync(salsifyInventoryFileName, cancellationToken);
            return await _sftpService.UploadFileAsync(fileData, salsifyInventoryFileName, cancellationToken);
        }

        private async Task<bool> ArchiveInventoryFile(String fileName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Archiving Inventory file {0}", fileName);
            var parts = fileName.Split('/').ToList();
            parts.Insert(parts.Count() - 1, _options.ArchiveFolder);

            return await _sftpService.MoveFileAsync(fileName, String.Join('/', parts.ToArray()), cancellationToken);
        }

        private async Task<IEnumerable<InventoryRecord>> FetchEcommerceInventoryFeed(string fileName, CancellationToken cancellationToken = default)
        {
            var csvStream = await _sftpService.DownloadFileAsync(fileName, cancellationToken);
            return ParseEcommerceInventoryFeed(csvStream);
        }

        private IEnumerable<InventoryRecord> ParseEcommerceInventoryFeed(byte[] csvStream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ",",
                Encoding = Encoding.UTF8,
                ShouldSkipRecord = record => record.Record.Count() <= 1
            };
            using var ms = new MemoryStream(csvStream);
            using var reader = new StreamReader(ms);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<CsvInventoryRecordMapping>();
            return csv.GetRecords<InventoryRecord>().ToList();
        }

        private async Task<IEnumerable<InventoryRecord>> FetchInventoryFeed(String fileName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching Inventory file {0}", fileName);
            var csvStream = await _sftpService.DownloadFileAsync(fileName, cancellationToken);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = "|",
                Encoding = Encoding.UTF8
            };
            using var ms = new MemoryStream(csvStream);
            using var reader = new StreamReader(ms);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<TxtInventoryRecordMapping>();
            return csv.GetRecords<InventoryRecord>().ToList();

        }

        private async Task<IDictionary<String, IList<InventoryItem>>> FetchInventoryItemLookup(string locationId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching Inventory From Shopify for {0}", locationId);
            var hash = new ConcurrentDictionary<string, IList<InventoryItem>>();
            var query = new ShopifyInventoryQuery(_options) {
                LocationId = locationId
            };
            await foreach(var level in _inventoryService.InventoryBulkLookup(query, cancellationToken))
            {
                if (string.IsNullOrEmpty(level.Item?.Sku))
                {
                    continue;
                }
                string key = level.Item.Sku;
                var item = level.Item;
                item.InventoryLevel = level;
                hash.AddOrUpdate(key, new List<InventoryItem> { item }, (k, v1) => v1.AsEnumerable().Append(item).ToList());
            }
            _logger.LogInformation("Fetched {0} Inventory Items", hash.Count);
            return hash;
        }

        private async Task<IDictionary<String, String>> FetchStoreIdLookup(String fileName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching StoreId Lookup file {0}", fileName);
            var csvStream = await _sftpService.DownloadFileAsync(fileName, cancellationToken);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = "|",
                Encoding = Encoding.UTF8
            };
            using var ms = new MemoryStream(csvStream);
            using var reader = new StreamReader(ms);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<CsvStoreLocationRecordMapping>();
            var records = csv.GetRecordsAsync<StoreLocationRecord>(cancellationToken);

            var results = new ConcurrentDictionary<string, string>();
            await foreach (var record in records)
            {
                results.TryAdd(record.StoreId, record.LocationId);
            }
            return results;
        }

        private IList<SalsifyProduct> GetLeafNodes(IEnumerable<SalsifyProduct> products)
        {
            var rval = new List<SalsifyProduct>();
            foreach (var product in products)
            {
                if (product.Children == null || !product.Children.Any())
                {
                    rval.Add(product);
                }
                else
                {
                    rval.AddRange(GetLeafNodes(product.Children));
                }
            }

            return rval;
        }
    }

    public static class IEnumerableExtensions
    {
        public static IEnumerable<List<T>> InSetsOf<T>(this IEnumerable<T> source, int max)
        {
            List<T> toReturn = new List<T>(max);
            foreach (var item in source)
            {
                toReturn.Add(item);
                if (toReturn.Count == max)
                {
                    yield return toReturn;
                    toReturn = new List<T>(max);
                }
            }
            if (toReturn.Any())
            {
                yield return toReturn;
            }
        }
    }
}

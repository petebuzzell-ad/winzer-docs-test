using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

using Winzer.Impl.Fulfillment;
using Winzer.Impl.CsvHelper;
using Winzer.Impl.Util;
using Winzer.Library;
using Winzer.Library.Error;
using Winzer.Library.Fulfillment;
using Winzer.Library.Util;
using Cql.Middleware.Library.Shopify.Order;
using Cql.Middleware.Library.Shopify.Shipping;
using Cql.Middleware.Library.Util;

using CsvHelper;
using CsvHelper.Configuration;
using Dasync.Collections;

namespace Winzer.Impl
{
    public class FulfillmentFeedService : IFulfillmentFeedService
    {
        private readonly ILogger<FulfillmentFeedService> _logger;
        private readonly IFileService _sftpService;
        private readonly IFileService _kwiSftpService;
        private readonly IFileServiceFactory _fileServiceFactory;
        private readonly IOrderService _orderService;
        private readonly IFulfillmentService _fulfillmentService;
        private readonly FulfillmentOptions _options;

        public FulfillmentFeedService(ILogger<FulfillmentFeedService> logger, IFileService sftpService, IFulfillmentService fulfillmentService, IOrderService orderService, FulfillmentOptions options, IFileServiceFactory fileServiceFactory)
        {
            _logger = logger;
            _sftpService = sftpService;
            _fileServiceFactory = fileServiceFactory;
            _fulfillmentService = fulfillmentService;
            _orderService = orderService;
            _options = options;
            _kwiSftpService = fileServiceFactory.GetFileService("KWI");
        }

        public async Task<IEnumerable<String>> FindFulfillmentFiles(string path, string pattern, CancellationToken cancellationToken = default)
        {
            Regex rgx = new Regex(pattern);
            var files = await _sftpService.ListDirectoryAsync(path, cancellationToken);
            var results = new List<String>();
            return files.Where((f) => rgx.IsMatch(f)).OrderBy(x => x);
        }

        public async Task<String> ImportFulfillmentFeed(string fileName, TestContext testContext, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Importing Fulfillments from {0}", fileName);

            var csvStream = await _sftpService.DownloadFileAsync(fileName, cancellationToken);

            if(csvStream.Length > _options.MaxFileSizeinBytes)
            {
                var message = $"{fileName} is {csvStream.Length / 1024}KB, which exceeds the maximum of {_options.MaxFileSizeinBytes / 1024}KB.";
                _logger.LogError(message);
                var moveMessage = "The file was moved to the error folder and will need to be reviewed.";
                if (!testContext.Dryrun)
                {
                    var success = await MoveFulfillmentFileToSubFolder(fileName, _options.ErrorFolder, cancellationToken);
                    if (!success)
                    {
                        moveMessage = $"Error moving file {fileName} to 'error' folder.";
                        _logger.LogError(moveMessage);
                    }
                }
                else
                {
                    moveMessage = "This is a dryrun so the file was not moved.";
                }
                throw new FileTooBigException($"{message} {moveMessage}");
            }

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
            csv.Context.RegisterClassMap<CsvFulfillmentRecordMapping>();

            var fulfillmentQueue = new List<FulfillmmentInput>();
            var kwiRecords = new List<KWINoteRecord>();

            foreach (var record in csv.GetRecords<FulfillmentRecord>())
            {
                var shopifyOrderName = record.OrderId.Substring(1, record.OrderId.Length - 1).TrimStart('0');
                var order = (await _orderService.LookupOrders(new ShopifyOrderQuery(_options)
                {
                    OrdersQuery = $"name:{shopifyOrderName}",
                    NumOrders = 1,
                    IncludeFulfillmentOrders = true,
                    IncludeShippingLine = true,
                    IncludeLineItems = true,
                    MetafieldNamespace = "cql",
                    LineItemVariantMetafieldsKey = "style_number",
                })).FirstOrDefault();
                if (order == null)
                {
                    _logger.LogError("Order not found for {orderId}.", record.OrderId);
                    continue;
                }
                if (!order.FulfillmentOrders.Any(f => f.Status == "OPEN"))
                    _logger.LogError("Open Fulfillment order not found for {orderId}.", record.OrderId);

                if (record.TrackingNumber != "EGIFTCARD")
                {
                    foreach (var fulfillmentOrder in order.FulfillmentOrders.Where(f => f.Status == "OPEN"))
                    {
                        if (fulfillmentOrder.DeliveryMethod.MethodType == "NONE")
                        {
                            fulfillmentQueue.Add(NoDeliveryFulfillment(fulfillmentOrder, record));
                        }
                        else
                        {
                            var styleNumbers = record.StyleNumbers.Split("|").Select(s => s.TrimStart('0'));
                            var fulfillmentStyleNumbers = fulfillmentOrder.LineItems.Select(l => l.LineItem.Variant?.Metafield?.Value?.ToString()?.TrimStart('0'));
                            if (!styleNumbers.Any(s => fulfillmentStyleNumbers.Contains(s)))
                            {
                                _logger.LogInformation("None of the style numbers are in the fulfillment order {fullfillmentOrderId} - {orderId}.", fulfillmentOrder.Id, record.OrderId);
                            }
                            else if (styleNumbers.Except(fulfillmentStyleNumbers).Any())
                            {
                                _logger.LogInformation("Some items aren't in the fulfillment order, {fullfillmentOrderId}.  Is {orderId} a partial?", fulfillmentOrder.Id, record.OrderId);
                            }
                            fulfillmentQueue.Add(ShippingFulfillment(fulfillmentOrder, record));
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Skipping EGIFTCARD line item for {orderId}.", record.OrderId);
                }
                kwiRecords.AddRange(AssembleKWINoteRecord(record, order));
            }

            if (fulfillmentQueue.Any() && !testContext.Dryrun)
            {
                _logger.LogInformation("{0} Fulfillments Queued", fulfillmentQueue.Count());

                await fulfillmentQueue.ParallelForEachAsync(async fulfillment =>
                {
                    var result2 = await _fulfillmentService.CreateFulfillment(fulfillment, cancellationToken);
                    if (result2 != null)
                    {
                        _logger.LogInformation("Created Fulfillment {id} for order {orderId}", result2.Id, fulfillment.OrderId);
                    }
                    else
                    {
                        _logger.LogError("Error: There was a problem creating a Fulfillment for Order {id}", fulfillment.OrderId);
                    }
                });
                _logger.LogInformation("{0} Fulfillments Created", fulfillmentQueue.Count());
            }

            await ExportKWINotes(kwiRecords, testContext, cancellationToken);
            if (!testContext.Dryrun)
            {
                var success = await MoveFulfillmentFileToSubFolder(fileName, _options.ArchiveFolder, cancellationToken);
                if (!success)
                {
                    _logger.LogError("Error Archiving File {fileName}", fileName);
                }
            }
            var dryrun = testContext.Dryrun ? "DryRun" : "OK";
            return $"{dryrun} - {fulfillmentQueue.Count()} Fulfillments Created";
        }


        public async Task<List<KWINoteRecord>> ExportKWINotes(List<KWINoteRecord> records, TestContext testContext, CancellationToken cancellationToken = default)
        {
            if (records.Any() && (!testContext.Dryrun || testContext.Dryrun && testContext.Test))
            {
                //Write Records as CSV
                using var ms = new MemoryStream();
                using var writer = new StreamWriter(ms, new UTF8Encoding(false));
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                    Delimiter = "|",
                    Encoding = Encoding.UTF8,
                    ShouldQuote = (args) => false
                };
                using var csv = new CsvWriter(writer, config);
                {
                    csv.Context.TypeConverterCache.RemoveConverter<string>();
                    csv.Context.TypeConverterCache.AddConverter<string>(new TruncatingStringConverter("|"));
                    await csv.WriteRecordsAsync(records, cancellationToken);
                    await csv.FlushAsync();
                    //EOF Line Count
                    writer.WriteLine(records.Count());
                    await writer.FlushAsync();
                    await ms.FlushAsync(cancellationToken);
                }

                var stream = ms.ToArray();

                //Upload CSV
                var exported_at = DateTime.Now;
                var fileName = $"/inbound/BRIGHTON-NOTE-{exported_at.ToString("MMddyy-HHmmss")}.txt";
                await _kwiSftpService.UploadFileAsync(stream, fileName, cancellationToken);
                _logger.LogInformation("Uploaded {fileName} to KWI FTP", fileName);
            }

            return records;
        }

        private IEnumerable<KWINoteRecord> AssembleKWINoteRecord(FulfillmentRecord record, ShopifyOrder order)
        {
            var result = new List<KWINoteRecord>();
            var fulfillmentDate = DateTime.UtcNow;

            int itemReference = 1;
            foreach (var lineItem in order.LineItems.Where(t => t.Vendor != "wrapin").OrderBy(t => t.Id))
            {
                result.Add(
                    new KWINoteRecord
                    {
                        ObjectName = "SHIPCONFIRM",
                        OrderNum = record.OrderId,
                        ItemRefNum = itemReference.ToString(),
                        ShipDate = fulfillmentDate.ConvertToDefaultTimeZone().ToString("MM/dd/yy"),
                        ItemShipMthd = CleanAlphanumeric(lineItem?.RequiresShipping ?? false ? order.ShippingLine?.Code : null),
                        PkgTrackNumUM = record.TrackingNumber,
                        Quantity = lineItem?.Quantity.ToString(),
                        InvoiceDate = order.CreatedAt.ConvertToDefaultTimeZone().ToString("MM/dd/yy"),
                    }
                );
                itemReference++;
            }
            return result;
        }

        private FulfillmmentInput ShippingFulfillment(FulfillmentOrder fulfillmentOrder, FulfillmentRecord record)
        {
            return new FulfillmmentInput
            {
                OrderId = record.OrderId,
                TrackingInfo = new FulfillmentTrackingInput
                {
                    Number = record.TrackingNumber,
                    Company = TranslateCarrier(record.TrackingNumber, record.Carrier)
                },
                NotifyCustomer = true,
                LineItemsByFulfillmentOrder = Enumerable.Repeat(new FulfillmentOrderLineItemsInput
                {
                    FulfillmentOrderId = fulfillmentOrder.Id
                }, 1)
            };
        }
        private FulfillmmentInput NoDeliveryFulfillment(FulfillmentOrder fulfillmentOrder, FulfillmentRecord record)
        {
            return new FulfillmmentInput
            {
                OrderId = record.OrderId,
                NotifyCustomer = false,
                LineItemsByFulfillmentOrder = Enumerable.Repeat(new FulfillmentOrderLineItemsInput
                {
                    FulfillmentOrderId = fulfillmentOrder.Id
                }, 1)
            };
        }

        private string TranslateCarrier(string trackingNumber, string carrier)
        {
            var codes = new List<string> { "UPS", "USPS" };
            if (codes.Contains(carrier.ToUpper()))
            {
                return carrier.ToUpper();
            }
            return trackingNumber.ToUpper().StartsWith("1Z") ? "UPS" : "USPS";
        }

        private async Task<bool> MoveFulfillmentFileToSubFolder(string fileName, string folder, CancellationToken cancellationToken = default)
        {
            var parts = fileName.Split('/').ToList();
            parts.Insert(parts.Count() - 1, folder);

            return await _sftpService.MoveFileAsync(fileName, String.Join('/', parts.ToArray()), cancellationToken);
        }

        private static string OrderIdFromName(string orderName)
        {
            return "3" + orderName.Replace("#", "").Trim().PadLeft(7, '0');
        }

        private static string? CleanAlphanumeric(string? value)
        {
            if (String.IsNullOrEmpty(value))
                return value;
            var r = new Regex("[^0-9A-Za-z -]*");
            return r.Replace(value, "");
        }
    }
}

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

using Winzer.Impl.Fulfillment;
using Winzer.Impl.CsvHelper;
using Winzer.Impl.Util;
using Winzer.Library;
using Winzer.Library.Loop;
using Winzer.Library.Util;

using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Order;
using Cql.Middleware.Library.Util;

using CsvHelper;
using CsvHelper.Configuration;
using Dasync.Collections;
using Newtonsoft.Json;

namespace Winzer.Impl
{
    public class ReturnExportService : IReturnExportService
    {
        private readonly ILogger<ReturnExportService> _logger;
        private readonly IFileService _kwiSftpService;
        private readonly IFileServiceFactory _fileServiceFactory;
        private readonly IOrderService _orderService;
        private readonly LoopReturnOptions _options;
        private readonly IReturnService _returnService;

        public ReturnExportService(ILogger<ReturnExportService> logger, IFileService sftpService, IOrderService orderService, LoopReturnOptions options, IFileServiceFactory fileServiceFactory, IReturnService returnService)
        {
            _logger = logger;
            _fileServiceFactory = fileServiceFactory;
            _orderService = orderService;
            _options = options;
            _kwiSftpService = _fileServiceFactory.GetFileService("KWI");
            _returnService = returnService;
        }

        public async Task<string> ExportReturns(DateTime since, DateTime to, TestContext testContext, CancellationToken cancellationToken = default)
        {
            var returns = await _returnService.LookupReturns(since, to, cancellationToken);
            var kwiRecords = new List<KWINoteRecord>();
            var returnMetadataLookup = new Dictionary<string, ReturnsMetadata>();
            var returnCount = 0;
            foreach (var r in returns.Where(x => x.State == "closed"))
            {
                try
                {
                    ShopifyOrder? order = null;
                    try
                    {
                        order = (await _orderService.LookupOrder(new ShopifyOrderQuery
                        {
                            OrderId = $"gid://shopify/Order/{r.ProviderOrderId.Trim()}",
                            IncludeLineItems = true,
                            IncludeFulfillments = true,
                            IncludeMetafields = true,
                            MetafieldNamespace = "cql",
                        }));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Order not found for {orderId}.", r.ProviderOrderId);
                    }

                    if (order == null)
                    {
                        _logger.LogError("Order not found for {orderId}.", r.ProviderOrderId);
                        continue;
                    }
                    var returnsMetafield = order.Metafields.Where(m => m.Key == _options.ReturnMetafieldKey).FirstOrDefault();
                    var returnsObject = LookupReturnsMetadata(returnsMetafield);
                    if (returnsObject.Returns.Contains(r.Id))
                    {
                        _logger.LogInformation("Return {id} has already been processed.", r.Id);
                        continue;
                    }

                    kwiRecords.AddRange(AssembleKWINoteRecord(r, order));

                    if (returnMetadataLookup.ContainsKey(order.Id))
                    {
                        returnMetadataLookup[order.Id].Returns.Add(r.Id);
                    }
                    else
                    {
                        returnsObject.Returns.Add(r.Id);
                        returnMetadataLookup.Add(order.Id, returnsObject);
                    }
                    returnCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing return for {orderId}.  Skipping", r.ProviderOrderId);
                }
            }

            if (kwiRecords.Any() && (!testContext.Dryrun || testContext.Dryrun && testContext.Test))
            {
                await ExportKWINotes(kwiRecords, cancellationToken);
            }
            if (returnMetadataLookup.Any() && !testContext.Dryrun)
            {
                var metafields = returnMetadataLookup.Select((kvp) => new ShopifyMetafieldsSetInput
                {
                    OwnerId = kvp.Key,
                    Namespace = "cql",
                    Key = _options.ReturnMetafieldKey,
                    Value = JsonConvert.SerializeObject(kvp.Value),
                    Type = "json"
                });
                await _orderService.SetMetafields(metafields);
            }

            var message = testContext.Dryrun ? $"DryRun - {returnCount} Returns Processed" : $"{returnCount} Returns Processed";
            _logger.LogInformation(message);
            return message;
        }

        private ReturnsMetadata LookupReturnsMetadata(ShopifyMetaField? returnsMetafield)
        {
            var returnsObject = new ReturnsMetadata();
            if (returnsMetafield != null && !String.IsNullOrEmpty(returnsMetafield.Value))
            {
                try
                {
                    var json = JsonConvert.DeserializeObject<ReturnsMetadata>(returnsMetafield.Value);
                    if (json != null)
                    {
                        returnsObject = json;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error parsing Returns Metadata from JSON Metafield: {json}", returnsMetafield.Value);
                }
            }
            return returnsObject;
        }

        public async Task<List<KWINoteRecord>> ExportKWINotes(List<KWINoteRecord> records, CancellationToken cancellationToken = default)
        {
            if (records.Any())
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
        private IEnumerable<KWINoteRecord> AssembleKWINoteRecord(LoopReturn loopReturn, ShopifyOrder order)
        {
            var result = new List<KWINoteRecord>();

            int itemReference = 1;
            foreach (var lineItem in order.LineItems.Where(t => t.Vendor != "wrapin").OrderBy(t => t.Id))
            {
                var loopLineItems = loopReturn.LineItems.Where(x => lineItem.Id.Split("/").Last() == x.ProviderLineItemId);
                if (loopLineItems.Any())
                {
                    foreach (var loopLineItem in loopLineItems)
                    {
                        var returnedAt = loopReturn.UpdatedAt.ToUniversalTime();
                        result.Add(
                            new KWINoteRecord
                            {
                                ObjectName = "RETURN",
                                OrderNum = order.OrderIdFromName(),
                                ItemRefNum = itemReference.ToString(),
                                ShipDate = returnedAt.ConvertToDefaultTimeZone().ToString("MM/dd/yy"),
                                ItemShipMthd = CleanAlphanumeric("Standard (Within 5-7 Business Days)"),
                                PkgTrackNumUM = loopReturn.TrackingNumber,
                                Quantity = "1",
                                InvoiceDate = order.CreatedAt.ConvertToDefaultTimeZone().ToString("MM/dd/yy"),
                                ReasonCode = ReasonCode(loopLineItem.ParentReturnReason),
                                ReturnCode = ReturnCode(loopReturn),
                                ReturnDate = returnedAt.ConvertToDefaultTimeZone().ToString("MM/dd/yy"),
                            }
                        );
                    }
                }
                itemReference++;
            }

            var missingLineItems = loopReturn.LineItems.Where(x => !order.LineItems.Any(l => l.Id.Split("/").Last() == x.ProviderLineItemId));
            if (missingLineItems.Any())
            {
                _logger.LogInformation("Missing {count} Line Items from the Return for order {order}:", missingLineItems.Count(), order.Id);
                foreach(var mli in missingLineItems)
                    _logger.LogInformation("- Missing item gid://shopify/LineItem/{id}", mli.ProviderLineItemId);
            }

            return result;
        }

        private static string? ReasonCode(string parentReturnReason)
        {
            var parentReturnCode = parentReturnReason.ToLowerInvariant();
            if (parentReturnCode.Contains("incorrect"))
                return "WOR";
            if (parentReturnCode.Contains("damaged"))
                return "DAM";
            if (parentReturnCode.Contains("experience"))
                return "WOR";
            if (parentReturnCode.Contains("fit"))
                return "WOR";
            if (parentReturnCode.Contains("something else"))
                return "WOR";
            if (parentReturnCode.Contains("like"))
                return "WOR";
            return "WOR";
        }

        private static string? ReturnCode(LoopReturn loopReturn)
        {
            if (loopReturn.Refund > 0)
            {
                return "RF";
            }
            if (loopReturn.Exchanges.Any())
            {
                return "EX";
            }
            if (loopReturn.GiftCard > 0)
            {
                return "SC";
            }
            return null;
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

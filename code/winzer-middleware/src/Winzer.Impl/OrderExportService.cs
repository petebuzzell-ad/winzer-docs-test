using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

using Winzer.Library;
using Winzer.Library.Crm;
using Winzer.Library.Util;
using Winzer.Impl.Order;
using Winzer.Impl.CsvHelper;
using Winzer.Impl.Util;
using Cql.Middleware.Library.Shopify.Order;
using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Util;

using CsvHelper;
using CsvHelper.Configuration;

using Newtonsoft.Json.Linq;

namespace Winzer.Impl;

public class OrderExportService : IOrderExportService
{
    private readonly ILogger<OrderExportService> _logger;
    private readonly IFileService _sftpService;
    private readonly IFileService _kwiSftpService;
    private readonly IFileServiceFactory _fileServiceFactory;
    private readonly IOrderService _orderService;
    private readonly IWinzerCrmService _brightonCrmService;
    private readonly OrderOptions _options;
    public OrderExportService(ILogger<OrderExportService> logger, IOrderService orderService, OrderOptions options, IWinzerCrmService brightonCrmService, IFileServiceFactory fileServiceFactory)
    {
        _logger = logger;
        _orderService = orderService;
        _options = options;
        _brightonCrmService = brightonCrmService;
        _fileServiceFactory = fileServiceFactory;
        _sftpService = _fileServiceFactory.DefaultFileService();
        _kwiSftpService = _fileServiceFactory.GetFileService("KWI");
    }

    public async Task<OrderExportResponse> ExportOrders(DateTime since, TestContext testContext, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting Orders since {0}", since);
        var orders = await LookupOrders(since, cancellationToken);
        var ordersForProcessing = new List<ShopifyOrder>();
        var invalidOrders = new List<ShopifyOrder>();
        foreach (var order in orders)
        {
            if(order.Tags.Contains(_options.ExportedTag))
            {
                _logger.LogWarning("Order {id} was already exported.  Skipping.", order.Id);
                continue;
            }

            if (ValidateOrder(order))
            {
                ordersForProcessing.Add(order);
            }
            else
            {
                _logger.LogWarning("Order {id} is not valid for export.", order.Id);
                invalidOrders.Add(order);
            }
        }
        if (invalidOrders.Any() && !testContext.Dryrun)
        {
            await TagOrders(invalidOrders, _options.NotExportedTag);
        }

        _logger.LogInformation("Found {orders} Orders to export", ordersForProcessing.Count());
        var succesfullOrders = Enumerable.Empty<ShopifyOrder>();
        if (ordersForProcessing.Any())
        {
            var kwiExportOrders = await ExportKWIOrders(ordersForProcessing, testContext, cancellationToken);
            var kwiRemainder = ordersForProcessing.Except(kwiExportOrders);
            if (kwiRemainder.Any())
                _logger.LogWarning("{count} orders were not successfully exported to KWI", kwiExportOrders.Count());

            var windxExportOrders = await ExportWindXOrders(ordersForProcessing, testContext, cancellationToken);
            var windxRemainder = ordersForProcessing.Except(windxExportOrders);
            if (windxRemainder.Any())
                _logger.LogWarning("{count} orders were not successfully exported to WindX", windxRemainder.Count());

            succesfullOrders = windxExportOrders.IntersectBy(kwiExportOrders.Select(o => o.Id), o => o.Id);
            if (!testContext.Dryrun)
            {
                // Only Taging orders that were successfully exported to both.
                await TagOrders(succesfullOrders, _options.ExportedTag);
            }
        }

        var failedOrders = await LookupFailedOrders(cancellationToken);
        var message = testContext.Dryrun ? $"DryRun - {succesfullOrders.Count()} Orders Exported" : $"{succesfullOrders.Count()} Orders Exported";
        _logger.LogInformation(message);

        return new OrderExportResponse
        {
            FailedOrderCount = failedOrders.Count(),
            Message = message
        };
    }

    public async Task<List<ShopifyOrder>> ExportKWIOrders(List<ShopifyOrder> orders, TestContext testContext, CancellationToken cancellationToken = default)
    {
        var processedOrders = new List<ShopifyOrder>();
        var records = new List<KWIOrderRecord>();
        foreach (var order in orders)
        {
            try
            {
                var orderRecords = AssembleKWIOrderRecord(order);
                records.AddRange(orderRecords);
                processedOrders.Add(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error: Order {id} failed to export", order.Id);
            }
        }

        _logger.LogInformation("Processed {orders} Orders for export to KWI", processedOrders.Count());
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
            var fileName = $"/inbound/BRIGHTON-ORDER-{exported_at.ToString("MMddyy-HHmmss")}.txt";
            await _kwiSftpService.UploadFileAsync(stream, fileName, cancellationToken);
            _logger.LogInformation("Uploaded {fileName} to FTP", fileName);
        }

        return processedOrders;
    }
    public async Task<List<ShopifyOrder>> ExportWindXOrders(List<ShopifyOrder> orders, TestContext testContext, CancellationToken cancellationToken = default)
    {
        var processedOrders = new List<ShopifyOrder>();
        var records = new List<WindXOrderRecord>();
        foreach (var order in orders)
        {
            try
            {
                var orderRecords = await AssembleWindXOrderRecord(order, cancellationToken);
                records.AddRange(orderRecords);
                processedOrders.Add(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error: Order {id} failed to export", order.Id);
            }
        }

        _logger.LogInformation("Processed {orders} Orders for export to WindX", processedOrders.Count());
        var fileName = String.Empty;
        if (records.Any() && (!testContext.Dryrun || testContext.Dryrun && testContext.Test))
        {
            //Write Records as CSV
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            {
                csv.Context.TypeConverterCache.RemoveConverter<string>();
                csv.Context.TypeConverterCache.AddConverter<string>(new DelimiterRemovingStringConverter());
                csv.WriteRecords(records);
                csv.Flush();
                writer.Flush();
                ms.Flush();
            }

            var stream = ms.ToArray();

            //Upload CSV
            var exported_at = DateTime.Now;
            fileName = $"/{_options.OrderFolder}/BRIGHT-SHOP-{exported_at.ToString("MMddyy-HHmmss")}.txt";
            await _sftpService.UploadFileAsync(stream, fileName);
            _logger.LogInformation("Uploaded {fileName} to FTP", fileName);
        }

        return processedOrders;
    }

    private async Task<bool> TagOrders(IEnumerable<ShopifyOrder> orders, string tag)
    {
        if (orders.Any())
        {
            foreach (var order in orders)
            {
                var newTags = order.Tags.ToList();
                _logger.LogInformation("{orderId} already has tags: [{newTags}]", order.Id, String.Join(",", newTags));
                if (newTags.Contains(tag))
                    continue;

                newTags.Add(tag);
                var updatedOrder = await _orderService.UpdateOrderTags(order.Id, newTags);
                _logger.LogInformation("Tagged {orderId} as {tag}", order.Id, tag);
                _logger.LogInformation("Updated {orderId} has tags: [{newTags}]", order.Id, String.Join(",", updatedOrder.Tags));
            }
        }
        return true;
    }

    private async Task<IEnumerable<ShopifyOrder>> LookupOrders(DateTime since, CancellationToken cancellationToken = default)
    {
        var lowRiskOrdersQuery = new ShopifyOrderQuery(_options)
        {
            OrdersQuery = $"(financial_status:paid AND tag_not:{_options.ExportedTag} AND tag_not:{_options.NotExportedTag} AND tag_not:{_options.LegacyOrderImportTag}) AND (riskLevel:LOW OR riskLevel:MEDIUM) AND (created_at:>'{since:s}')",
            IncludeShippingAddress = true,
            IncludeBillingAddress = true,
            IncludeTransactions = true,
            IncludeShippingLine = true,
            IncludeLineItems = true,
            IncludeLineItemVariant = true,
            MetafieldNamespace = "cql",
            LineItemVariantMetafieldsKey = "style_number",
        };
        var lowRiskOrders = _orderService.LookupOrders(lowRiskOrdersQuery, cancellationToken);

        var manualOverrideOrdersQuery = new ShopifyOrderQuery(_options)
        {
            OrdersQuery = $"(financial_status:paid AND tag_not:{_options.ExportedTag} AND tag_not:{_options.NotExportedTag} AND tag_not:{_options.LegacyOrderImportTag}) AND (tag:{_options.FraudOverideTag}) AND (created_at:>'{since:s}')",
            IncludeShippingAddress = true,
            IncludeBillingAddress = true,
            IncludeTransactions = true,
            IncludeShippingLine = true,
            IncludeLineItems = true,
            IncludeLineItemVariant = true,
            MetafieldNamespace = "cql",
            LineItemVariantMetafieldsKey = "style_number",
        };
        var manualOverrideOrders = _orderService.LookupOrders(manualOverrideOrdersQuery, cancellationToken);

        await Task.WhenAll(lowRiskOrders, manualOverrideOrders);
        _logger.LogInformation("Found {orders} Low Risk Orders to export", lowRiskOrders.Result.Count());
        _logger.LogInformation("Found {orders} Manually Overidden Orders to export", manualOverrideOrders.Result.Count());
        return lowRiskOrders.Result.UnionBy(manualOverrideOrders.Result, (o) => o.Id);
    }

    private async Task<IEnumerable<ShopifyOrder>> LookupFailedOrders(CancellationToken cancellationToken = default)
    {
        // Search For Failed Orders
        var failedOrdersQuery = new ShopifyOrderQuery(_options)
        {
            OrdersQuery = $"(NOT financial_status:paid) AND tag_not:{_options.ExportedTag} AND tag_not:{_options.NotExportedTag} AND tag_not:{_options.LegacyOrderImportTag} AND (created_at:>'{DateTime.Now.AddHours(-1 * _options.FailedOrdersFromHoursAgo):s}' created_at:<='{DateTime.Now.AddHours(-1 * _options.FailedOrdersToHoursAgo):s})'"
        };

        var failedOrders = await _orderService.LookupFailedOrders(failedOrdersQuery, cancellationToken);
        if (failedOrders.Any())
        {
            _logger.LogWarning("Warning: {count} orders have payment issues preventing export", failedOrders.Count());
            _logger.LogWarning($"Found in range from '{DateTime.Now.AddHours(-1 * _options.FailedOrdersFromHoursAgo):s}' to '{DateTime.Now.AddHours(-1 * _options.FailedOrdersToHoursAgo):s}'");
            foreach (var missedOrder in failedOrders)
            {
                _logger.LogWarning("Warning: {id} has payment issues preventing export", missedOrder.Id);
            }
        }
        return failedOrders;
    }

    private bool ValidateOrder(ShopifyOrder order)
    {
        var valid = true;
        foreach (var lineItem in order.LineItems.Where(t => t.Vendor != "wrapin"))
        {
            if (!ValidateSku(lineItem.Sku))
            {
                valid = false;
            }
        }
        return valid;
    }

    private bool ValidateSku(string? sku)
    {
        if (sku == null)
            return false;

        return Regex.IsMatch(sku, @"^[0-9]*$") || IsClutchSku(sku);
    }

    private bool IsClutchSku(string? sku)
    {
        if (sku == null)
            return false;

        switch (sku)
        {
            case "cltchg_25":
            case "cltchg_50":
            case "cltchg_75":
            case "cltchg_100":
            case "cltchg_150":
            case "cltchg_200":
            case "cltchg_300":
            case "cltchg_500":
                return true;
            default:
                return false;
        }
    }

    private string? MapClutchSku(string? sku)
    {
        if (sku == null)
            return sku;

        switch (sku)
        {
            case "cltchg_25":
                return "843036862149";
            case "cltchg_50":
                return "843036862156";
            case "cltchg_75":
                return "843036862163";
            case "cltchg_100":
                return "843036862170";
            case "cltchg_150":
                return "843036862187";
            case "cltchg_200":
                return "843036862194";
            case "cltchg_300":
                return "843036862200";
            case "cltchg_500":
                return "843036862217";
            default:
                return sku;
        }
    }

    private async Task<IEnumerable<WindXOrderRecord>> AssembleWindXOrderRecord(ShopifyOrder order, CancellationToken cancellationToken)
    {
        var result = new List<WindXOrderRecord>();
        var customerId = await _brightonCrmService.LookupCustomerId(order.Email, cancellationToken);
        var ccTransaction = order.Transactions.Successfull().AuthOrCapture().Where(t => t.Gateway == "shopify_payments").FirstOrDefault();
        var paypalTransaction = order.Transactions.Successfull().AuthOrCapture().Where(t => t.Gateway == "paypal").FirstOrDefault();
        var giftCardTransactions = order.Transactions.Successfull().AuthOrCapture().Where(t => t.Gateway == "gift_card");
        var transaction = ccTransaction ?? paypalTransaction;
        var giftWrap = order.LineItems.Where(t => t.Vendor == "wrapin").FirstOrDefault();
        foreach (var lineItem in order.LineItems.Where(t => t.Vendor != "wrapin").OrderBy(x => x.Id))
        {
            result.Add(
                new WindXOrderRecord
                {
                    OrderNumber = OrderIdFromName(order.Name),

                    ShipToFirstName = order.ShippingAddress?.FirstName,
                    ShipToLastName = order.ShippingAddress?.LastName,
                    ShipToAddress1 = order.ShippingAddress?.Address1,
                    ShipToAddress2 = order.ShippingAddress?.Address2,
                    ShipToCity = order.ShippingAddress?.City,
                    ShipToState = order.ShippingAddress?.ProvinceCode,
                    ShipToZip = order.ShippingAddress?.Zip,
                    ShipToPhone = order.ShippingAddress?.Phone ?? order.Phone,

                    BillToFirstName = order.BillingAddress?.FirstName,
                    BillToLastName = order.BillingAddress?.LastName,
                    BillToAddress1 = order.BillingAddress?.Address1,
                    BillToAddress2 = order.BillingAddress?.Address2,
                    BillToCity = order.BillingAddress?.City,
                    BillToState = order.BillingAddress?.ProvinceCode,
                    BillToZip = order.BillingAddress?.Zip,
                    BillToPhone = order.BillingAddress?.Phone ?? order.Phone,

                    CustomerId = customerId,

                    PaymentMethod = transaction?.FormattedGateway, //unused
                    LastFour = CleanAccountNumber(transaction?.AccountNumber),
                    Total = order.TotalPriceSet.ShopMoney.Amount.ToString("G"),
                    CouponName = order.DiscountCode, //unused
                    ShipMethod = order.ShippingLine?.Code,
                    ShippingCost = order.ShippingLine?.DiscountedPriceSet.ShopMoney.Amount.ToString("G"),
                    Upc = MapClutchSku(lineItem.Sku),
                    StyleNumber = lineItem.Variant?.Metafield?.Value?.ToString(),
                    Size = lineItem.Variant?.Option2, //unused
                    Color = lineItem.Variant?.Option1, //unused
                    UnitPrice = lineItem.OriginalUnitPriceSet.ShopMoney.Amount.ToString("G"),
                    SoldFor = lineItem.DiscountedUnitPriceSet.ShopMoney.Amount.ToString("G"),
                    Qty = lineItem.Quantity.ToString(),
                    ProductName = lineItem.Name, //unused
                    GiftMessage = order.CustomAttributes.Where(x => x.Key == "gift-message-note").FirstOrDefault()?.Value,
                    Email = order.Email,
                    OrderDate = order.CreatedAt.ConvertToPacificTimeZone().ToString("yyyy-MM-dd"),
                    OrderTime = order.CreatedAt.ConvertToPacificTimeZone().ToString("hh:mm:ss tt"),
                    Tax = order.TotalTaxSet?.ShopMoney.Amount.ToString("G"),

                    GiftCardTotal = giftCardTransactions.Sum(x => x.AmountSet.ShopMoney.Amount).ToString("G"),
                    Charmbuild = "0", //1 if charmbuilder app was used
                    GiftWrap = giftWrap != null ? "Y" : "N", // Y if gift wrap option selected
                    GemComment = "", //styles and colors for YTC (Your True Color - customized jewelry) orders
                    GiftCardInfo = GiftCardInfo(lineItem.CustomAttributes), // name, email)
                    Transnumber = lineItem.CustomAttributes.Where(x => x.Key == "_designId").FirstOrDefault()?.Value, //confirmation number for a personalized locket.
                }
            );
        }
        return result;
    }

    private string? GiftCardInfo(IEnumerable<ShopifyAttribute> customAttributes)
    {
        if (customAttributes.Any())
        {
            var first = customAttributes.Where(x => x.Key == "Recipient first name").FirstOrDefault()?.Value;
            var last = customAttributes.Where(x => x.Key == "Recipient last name").FirstOrDefault()?.Value;
            var email = customAttributes.Where(x => x.Key == "Recipient email address").FirstOrDefault()?.Value;

            if (!String.IsNullOrEmpty(first) || !String.IsNullOrEmpty(last) || !String.IsNullOrEmpty(email))
            {
                return String.Join('|', $"{first} {last}".Trim(), email);
            }
        }
        return null;
    }

    private IEnumerable<KWIOrderRecord> AssembleKWIOrderRecord(ShopifyOrder order)
    {
        var result = new List<KWIOrderRecord>();
        var ccTransaction = order.Transactions.Successfull().AuthOrCapture().Where(t => t.Gateway == "shopify_payments").FirstOrDefault();
        var jobj = JObject.Parse(ccTransaction?.ReceiptJson ?? "{}");
        var cardBrand = (string?)jobj.SelectToken("charges.data[0].payment_method_details.card.brand");
        var cardExpMonth = (int?)jobj.SelectToken("charges.data[0].payment_method_details.card.exp_month");
        var cardExpYear = (int?)jobj.SelectToken("charges.data[0].payment_method_details.card.exp_year");

        var paypalTransaction = order.Transactions.Successfull().AuthOrCapture().Where(t => t.Gateway == "paypal").FirstOrDefault();
        var transaction = ccTransaction ?? paypalTransaction;
        var giftCardTransactions = order.Transactions.Successfull().AuthOrCapture().Where(t => t.Gateway == "gift_card");
        var firstGiftCardTransaction = giftCardTransactions.FirstOrDefault();
        var secondGiftCardTransaction = giftCardTransactions.Skip(1).FirstOrDefault();
        var giftWrap = order.LineItems.Where(t => t.Vendor == "wrapin").FirstOrDefault();
        int itemReference = 1;
        foreach (var lineItem in order.LineItems.Where(t => t.Vendor != "wrapin").OrderBy(x => x.Id))
        {
            result.Add(
                new KWIOrderRecord
                {
                    OrderNumber = OrderIdFromName(order.Name),
                    OrderDate = order.CreatedAt.ConvertToDefaultTimeZone().ToString("MM/dd/yy"),
                    OrderTime = order.CreatedAt.ConvertToDefaultTimeZone().ToString("hh:mm"),
                    OrderSalesTax = order.LineItems.SelectMany(l => l.TaxLines.Select(t => t.PriceSet.ShopMoney)).Sum(m => m.Amount).ToString("G"),
                    OrderShipCharge = order.ShippingLine?.DiscountedPriceSet.ShopMoney.Amount.ToString("G"),
                    OrderMerchandiseTotal = order.SubtotalPriceSet?.ShopMoney.Amount.ToString("G"),

                    BillToFirstName = order.BillingAddress?.FirstName,
                    BillToLastName = order.BillingAddress?.LastName,
                    BillToAddress1 = order.BillingAddress?.Address1,
                    BillToAddress2 = order.BillingAddress?.Address2,
                    BillToCity = order.BillingAddress?.City,
                    BillToState = order.BillingAddress?.ProvinceCode,
                    BillToZip = order.BillingAddress?.Zip,
                    BillToCountry = order.BillingAddress?.CountryCodeV2,
                    BillToTelephone = CleanNumber(order.BillingAddress?.Phone ?? order.Phone),
                    BillToEmail = order.Email,

                    PaymentType1 = PaymentType(cardBrand ?? transaction?.Gateway),
                    PaymentType1Num = CleanAccountNumber(transaction?.AccountNumber),
                    PaymentType1Expires = CreditCardExpiration(cardExpMonth, cardExpYear),
                    PaymentType1AuthCode = transaction?.AuthorizationCode,
                    PaymentType1AuthDate = order.CreatedAt.ConvertToDefaultTimeZone().ToString("MM/dd/yy"),
                    PaymentType1Total = transaction?.AmountSet.ShopMoney.Amount.ToString("G"),

                    PaymentType2 = firstGiftCardTransaction != null ? "Gift Card" : null,
                    PaymentType2Num = CleanAccountNumber(firstGiftCardTransaction?.AccountNumber),
                    PaymentType2Expires = firstGiftCardTransaction != null ? "1234" : null,
                    PaymentType2AuthCode = firstGiftCardTransaction != null ? "1234567890" : null,
                    PaymentType2AuthDate = firstGiftCardTransaction != null ? order.CreatedAt.ConvertToDefaultTimeZone().ToString("MM/dd/yy") : null,
                    PaymentType2Total = firstGiftCardTransaction?.AmountSet.ShopMoney.Amount.ToString("G"),

                    PaymentType3 = secondGiftCardTransaction != null ? "Gift Card" : null,
                    PaymentType3Num = CleanAccountNumber(secondGiftCardTransaction?.AccountNumber),
                    PaymentType3Expires = secondGiftCardTransaction != null ? "1234" : null,
                    PaymentType3AuthCode = secondGiftCardTransaction != null ? "1234567890" : null,
                    PaymentType3AuthDate = secondGiftCardTransaction != null ? order.CreatedAt.ConvertToDefaultTimeZone().ToString("MM/dd/yy") : null,
                    PaymentType3Total = secondGiftCardTransaction?.AmountSet.ShopMoney.Amount.ToString("G"),

                    ItemReferenceNum = itemReference.ToString(),
                    ItemId = CleanSku(MapClutchSku(lineItem.Sku)),
                    ItemDescription = lineItem.Name,
                    Personalization = giftWrap != null ? "1" : "0", // 1 if gift wrap option selected

                    ItemPrice = DiscountedItemPrice(lineItem),
                    ItemSalesTax = RoundMoney(lineItem.TaxLines.Sum(t => t.PriceSet.ShopMoney.Amount) / lineItem.Quantity),
                    ItemQuantity = lineItem.Quantity.ToString(),
                    ItemShipMethod = CleanAlphanumeric(lineItem.RequiresShipping ? order.ShippingLine?.Code : null),

                    ShipToFirstName = order.ShippingAddress?.FirstName,
                    ShipToLastName = order.ShippingAddress?.LastName,
                    ShipToAddress1 = order.ShippingAddress?.Address1,
                    ShipToAddress2 = order.ShippingAddress?.Address2,
                    ShipToCity = order.ShippingAddress?.City,
                    ShipToState = order.ShippingAddress?.ProvinceCode,
                    ShipToZip = order.ShippingAddress?.Zip,
                    ShipToCountry = order.ShippingAddress?.CountryCodeV2,
                    ShipToTelephone = CleanNumber(order.ShippingAddress?.Phone ?? order.Phone),
                    ShipToEmail = order.Email,

                    ShipTax = order.ShippingLine?.TaxLines.Sum(x => x.PriceSet.ShopMoney.Amount).ToString("G"),
                    SalesPromotion = order.DiscountCode
                }
            );
            itemReference++;
        }
        return result;
    }

    private static string? DiscountedItemPrice(ShopifyLineItem lineItem)
    {
        var lineItemDiscountAmount = lineItem.DiscountAllocations
            .Where(d => d.DiscountApplication.TargetSelection != "EXPLICIT" && d.DiscountApplication.TargetType == "LINE_ITEM" && d.DiscountApplication.AllocationMethod == "ACROSS")
            .Sum(d => d.AllocatedAmountSet.ShopMoney.Amount);
        var discountedLineItemPrice = lineItem.DiscountedTotalSet.ShopMoney.Amount - lineItemDiscountAmount;
        return RoundMoney(discountedLineItemPrice / lineItem.Quantity);
    }

    private static string? PaymentType(string? value)
    {
        if (value == null)
            return null;

        switch (value)
        {
            case "visa":
                return "VI";
            case "mastercard":
                return "MC";
            case "amex":
                return "AX";
            case "discover":
                return "DI";
            case "paypal":
                return "PP";
            default:
                return "DC";
        }
    }

    private static string? CreditCardExpiration(int? expMonth, int? expYear)
    {
        if (expMonth == null || expYear == null)
            return null;

        var year = expYear.Value.ToString();
        var month = expMonth.Value.ToString();
        return $"{month.PadLeft(2, '0')}{year.Substring(Math.Max(year.Length - 2, 0))}";
    }

    private static string? RoundMoney(decimal? value)
    {
        if (value == null)
            return null;
        return Math.Round(value.Value, 2, MidpointRounding.AwayFromZero).ToString("G");
    }
    private static string? CleanSku(string? value)
    {
        if (String.IsNullOrEmpty(value) || value.Length < 2)
            return value;
        return value.Substring(1, value.Length - 2);
    }

    private static string? CleanNumber(string? value)
    {
        if (String.IsNullOrEmpty(value))
            return value;
        var r = new Regex("[^0-9]*");
        return r.Replace(value, "");
    }

    private static string? CleanAlphanumeric(string? value)
    {
        if (String.IsNullOrEmpty(value))
            return value;
        var r = new Regex("[^0-9A-Za-z]*");
        return r.Replace(value, "");
    }

    private static string? CleanAccountNumber(string? value)
    {
        if (String.IsNullOrEmpty(value))
            return value;
        return value.Replace("•", "").Trim();
    }

    private static string OrderIdFromName(string orderName)
    {
        return "3" + orderName.Replace("#", "").Trim().PadLeft(7, '0');
    }
}

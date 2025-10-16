using System.Text.Json.Serialization;

namespace Winzer.Library.Loop;
// LoopReturn myDeserializedClass = JsonConvert.DeserializeObject<List<LoopReturn>>(myJsonResponse);

public class LoopReturn
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = String.Empty;

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("edited_at")]
    public object? EditedAt { get; set; }

    [JsonPropertyName("total")]
    public decimal? Total { get; set; }

    [JsonPropertyName("order_id")]
    public string? OrderId { get; set; }

    [JsonPropertyName("order_name")]
    public string? OrderName { get; set; }

    [JsonPropertyName("provider_order_id")]
    public string ProviderOrderId { get; set; } = String.Empty;

    [JsonPropertyName("order_number")]
    public string? OrderNumber { get; set; }

    [JsonPropertyName("customer")]
    public string? Customer { get; set; }

    [JsonPropertyName("multi_currency")]
    public bool? MultiCurrency { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("return_product_total")]
    public decimal? ReturnProductTotal { get; set; }

    [JsonPropertyName("return_discount_total")]
    public decimal? ReturnDiscountTotal { get; set; }

    [JsonPropertyName("return_tax_total")]
    public decimal? ReturnTaxTotal { get; set; }

    [JsonPropertyName("return_total")]
    public decimal? ReturnTotal { get; set; }

    [JsonPropertyName("return_credit_total")]
    public decimal? ReturnCreditTotal { get; set; }

    [JsonPropertyName("exchange_product_total")]
    public decimal? ExchangeProductTotal { get; set; }

    [JsonPropertyName("exchange_discount_total")]
    public decimal? ExchangeDiscountTotal { get; set; }

    [JsonPropertyName("exchange_tax_total")]
    public decimal? ExchangeTaxTotal { get; set; }

    [JsonPropertyName("exchange_total")]
    public decimal? ExchangeTotal { get; set; }

    [JsonPropertyName("exchange_credit_total")]
    public decimal? ExchangeCreditTotal { get; set; }

    [JsonPropertyName("gift_card")]
    public decimal? GiftCard { get; set; }

    [JsonPropertyName("gift_card_order_name")]
    public string? GiftCardOrderName { get; set; }

    [JsonPropertyName("gift_card_order_id")]
    public string? GiftCardOrderId { get; set; }

    [JsonPropertyName("handling_fee")]
    public decimal? HandlingFee { get; set; }

    [JsonPropertyName("refund")]
    public decimal? Refund { get; set; }

    [JsonPropertyName("upsell")]
    public decimal? Upsell { get; set; }

    [JsonPropertyName("line_items")]
    public List<LineItem> LineItems { get; set; } = new List<LineItem>();

    [JsonPropertyName("exchanges")]
    public List<Exchange> Exchanges { get; set; } = new List<Exchange>();

    [JsonPropertyName("carrier")]
    public string? Carrier { get; set; }

    [JsonPropertyName("tracking_number")]
    public string? TrackingNumber { get; set; }

    [JsonPropertyName("label_status")]
    public string? LabelStatus { get; set; }

    [JsonPropertyName("status_page_url")]
    public string? StatusPageUrl { get; set; }

    [JsonPropertyName("label_updated_at")]
    public object? LabelUpdatedAt { get; set; }

    [JsonPropertyName("destination_id")]
    public object? DestinationId { get; set; }

    [JsonPropertyName("return_method")]
    public object? ReturnMethod { get; set; }
}

public class Exchange
{
    [JsonPropertyName("exchange_id")]
    public string? ExchangeId { get; set; }

    [JsonPropertyName("exchange_order_id")]
    public object? ExchangeOrderId { get; set; }

    [JsonPropertyName("exchange_order_name")]
    public string? ExchangeOrderName { get; set; }

    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }

    [JsonPropertyName("variant_id")]
    public string? VariantId { get; set; }

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("discount")]
    public decimal? Discount { get; set; }

    [JsonPropertyName("tax")]
    public decimal? Tax { get; set; }

    [JsonPropertyName("total")]
    public decimal? Total { get; set; }

    [JsonPropertyName("out_of_stock")]
    public bool? OutOfStock { get; set; }

    [JsonPropertyName("out_of_stock_resolution")]
    public object? OutOfStockResolution { get; set; }
}

public class LineItem
{
    [JsonPropertyName("line_item_id")]
    public string? LineItemId { get; set; }

    [JsonPropertyName("provider_line_item_id")]
    public string? ProviderLineItemId { get; set; }

    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }

    [JsonPropertyName("variant_id")]
    public string? VariantId { get; set; }

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("discount")]
    public decimal? Discount { get; set; }

    [JsonPropertyName("tax")]
    public decimal? Tax { get; set; }

    [JsonPropertyName("refund")]
    public decimal? Refund { get; set; }

    [JsonPropertyName("returned_at")]
    public string? ReturnedAt { get; set; }

    [JsonPropertyName("exchange_variant")]
    public string? ExchangeVariant { get; set; }

    [JsonPropertyName("return_reason")]
    public string? ReturnReason { get; set; }

    [JsonPropertyName("parent_return_reason")]
    public string ParentReturnReason { get; set; } = String.Empty;

    [JsonPropertyName("return_comment")]
    public string? ReturnComment { get; set; }

    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }

    [JsonPropertyName("outcome")]
    public string? Outcome { get; set; }
}

using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable IDE1006 // Naming Styles
namespace Cql.Middleware.Impl.Shopify.GraphQL.Models
{
    public class ShopifyGraphQLProductUpdateInput
    {
        public string? category {  get; set; }
        public string[]? collectionsToJoin { get; set; }
        public string[]? collectionsToLeave { get; set; }
        public string? descriptionHtml { get; set; }
        public string? giftCardTemplateSuffix { get; set; }
        public string? handle { get; set; }
        public string? id { get; set; }
        public ShopifyGraphQLMetafieldInput[]? metafields { get; set; }
        public string? productType { get; set; }
        public bool? redirectNewHandle { get; set; }
        public bool? requiresSellingPlan { get; set; }
        public ShopifyGraphQLSEOInput? seo { get; set; }

        public string? status { get; set; }
        public string? tags { get; set; }
        public string? templateSuffix { get; set; }
        public string? title { get; set; }

        public string? vendor { get; set; }
    }


    public class ShopifyGraphQLProductInput
    {
        public string? bodyHtml { get; set; }
        public string[]? collectionsToJoin { get; set; }
        public string[]? collectionsToLeave { get; set; }
        public string? customProductType { get; set; }
        public string? descriptionHtml { get; set; }
        public bool? giftCard { get; set; }
        public string? handle { get; set; }
        public string? id { get; set; }
        //public ShopifyGraphQLImageInput[]? media { get; set; }
        public ShopifyGraphQLMetafieldInput[]? metafields { get; set; }
        public ShopifyGraphQLOptionCreateInput[]? productOptions { get; set; }
        public string? productType { get; set; }
        public ShopifyGraphQLSEOInput? seo { get; set; }
        public ShopifyGraphQLStandardizedProductTypeInput? standardizedProductType { get; set; }
        public string? status { get; set; }
        public string? title { get; set; }
        //public ShopifyGraphQLProductVariantInput[]? variants { get; set; }
        public string? vendor { get; set; }
        public string? templateSuffix { get; set; }

        public bool? published { get; set; } // Note that this is deprecated, but seems to be the only way to publish a product right now..

        public string? tags { get; set; }
    }

    public class ShopifyGraphQLMetafieldInput
    {
        public string? description { get; set; }

        public string? id { set; get; }

        public string? key { get; set; }

        [Newtonsoft.Json.JsonProperty("namespace")]
        public string? Namespace { get; set; }

        [Newtonsoft.Json.JsonProperty("type")]
        public string? Type { get; set; }

        public string? value { get; set; }
    }

    public class ShopifyGraphQLProductVariantAppendMediaInput
    {
        public string variantId { get; set; }
        public string[] mediaIds { get; set; }
    }

    public class ShopifyGraphQLVariantOptionValueInput
    {
        public string id { get; set; }

        public string name { get; set; }

        public string optionId { get; set; }

        public string optionName { get; set; }
    }

    public class ShopifyGraphQLProductVariantBulkInput
    {
        public string? barcode { get; set; }
        public decimal? compareAtPrice { get; set; }

        public string? id { get; set; }

        public string? mediaId { get; set; }

        public string? mediaSrc { get; set; }

        public ShopifyGraphQLMetafieldInput[]? metafields { get; set; }

        public ShopifyGraphQLVariantOptionValueInput[]? optionValues { get; set; }

        public decimal? price { get; set; }

        public string? taxCode { get; set; }

        public bool? taxable { get; set; }

        public ShopifyGraphQLInventoryItemInput? inventoryItem { get; set; }
    }

    public class ShopifyGraphQLProductVariantInput
    {
        public string? barcode { get; set; }
        public decimal? compareAtPrice { get; set; }
        public string? fulfillmentServiceId { get; set; }

        public string? harmonizedSystemCode { get; set; }

        public string? id { get; set; }

        public string? mediaId { get; set; }

        public string? mediaSrc { get; set; }

        public ShopifyGraphQLMetafieldInput[]? metafields { get; set; }

        public string[]? options { get; set; }

        public int? position { get; set; }

        public decimal? price { get; set; }

        public string? productId { get; set; }

        public bool? requiresShipping { get; set; }

        public string? sku { get; set; }

        public string? taxCode { get; set; }

        public bool? taxable { get; set; }

        public decimal? weight { get; set; }

        public string? weightUnit { get; set; }

        public ShopifyGraphQLInventoryItemInput? inventoryItem { get; set; }
    }

    public class ShopifyGraphQLInventoryItemInput
    {
        public decimal? cost { get; set; }

        public bool? tracked { get; set; } = true;

        public string? sku { get; set; }

        public ShopifyGraphQLInventoryMeasurementInput? measurement { get; set; }
    }

    public class ShopifyGraphQLInventoryMeasurementInput
    {
        public ShopifyGraphQLWeightInput? weight { get; set; }
    }

    public class ShopifyGraphQLWeightInput
    {
        public string? unit { get; set; }
        public decimal? value { get; set; }

    }

    public class ShopifyGraphQLImageInput
    {
        public ShopifyGraphQLImageInput() {
            mediaContentType = "IMAGE";
        }

        public string mediaContentType { get; set; }

        public string? alt { get; set; }

        public string? id { get; set; }

        public string? orignalSource { get; set; }
    }

    public class ShopifyGraphQLSEOInput
    {
        public string? description { get; set; }

        public string? title { get; set; }
    }

    public class ShopifyGraphQLStandardizedProductTypeInput
    {
        public string? productTaxonomyNodeId { get; set; }
    }
/*
    public class ShopifyGraphQLProductAppendImagesInput
    {
        public ShopifyGraphQLProductAppendImagesInput(string productId, IEnumerable<ShopifyGraphQLImageInput> images)
        {
            this.id = productId;
            this.images = images.ToArray();
        }

        public string id { get; set; }

        public ShopifyGraphQLImageInput[] images { get; set; }
    }
*/
    public class ShopifyGraphQLProductCreateMediaInput
    {
        public string alt { get; set; }

        public string mediaContentType { get; set; }

        public string originalSource { get; set; }
    }

    public class ShopifyGraphQLProductVariantPositionInput
    {
        public ShopifyGraphQLProductVariantPositionInput(string id, int position)
        {
            this.id = id;
            this.position = position;
        }

        public string id { get; set; }

        public int position { get; set; }
    }
}

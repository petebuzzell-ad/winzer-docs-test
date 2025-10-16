using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable IDE1006 // Naming Styles
namespace Winzer.Impl.Salsify
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class SalsifyExportAttribute
    {
        [JsonProperty("salsify:id")]
        public string? SalsifyId { get; set; }

        [JsonProperty("salsify:name")]
        public string? SalsifyName { get; set; }

        [JsonProperty("salsify:data_type")]
        public string? SalsifyDataType { get; set; }

        [JsonProperty("salsify:role")]
        public string? SalsifyRole { get; set; }

        [JsonProperty("salsify:entity_types")]
        public List<string>? SalsifyEntityTypes { get; set; }

        [JsonProperty("salsify:is_facetable")]
        public bool SalsifyIsFacetable { get; set; }

        [JsonProperty("salsify:attribute_group")]
        public string? SalsifyAttributeGroup { get; set; }

        [JsonProperty("salsify:position")]
        public string? SalsifyPosition { get; set; }

        [JsonProperty("salsify:help_text")]
        public string? SalsifyHelpText { get; set; }

        [JsonProperty("salsify:created_at")]
        public DateTime SalsifyCreatedAt { get; set; }

        [JsonProperty("salsify:updated_at")]
        public DateTime SalsifyUpdatedAt { get; set; }

        [JsonProperty("salsify:type")]
        public string? SalsifyType { get; set; }

        [JsonProperty("salsify:system_id")]
        public string? SalsifySystemId { get; set; }
    }

    public class SalsifyExportAttributeValue
    {
        [JsonProperty("salsify:id")]
        public string? SalsifyId { get; set; }

        [JsonProperty("salsify:attribute_id")]
        public string? SalsifyAttributeId { get; set; }

        [JsonProperty("salsify:name")]
        public string? SalsifyName { get; set; }

        [JsonProperty("salsify:created_at")]
        public DateTime SalsifyCreatedAt { get; set; }

        [JsonProperty("salsify:updated_at")]
        public DateTime SalsifyUpdatedAt { get; set; }

        [JsonProperty("salsify:parent_id")]
        public string? SalsifyParentId { get; set; }
    }

    public class SalsifyExportDigitalAsset
    {
        [JsonProperty("salsify:id")]
        public string? SalsifyId { get; set; }

        [JsonProperty("salsify:url")]
        public string? SalsifyUrl { get; set; }

        [JsonProperty("salsify:source_url")]
        public string? SalsifySourceUrl { get; set; }

        [JsonProperty("salsify:name")]
        public string? SalsifyName { get; set; }

        [JsonProperty("salsify:created_at")]
        public DateTime SalsifyCreatedAt { get; set; }

        [JsonProperty("salsify:updated_at")]
        public DateTime SalsifyUpdatedAt { get; set; }

        [JsonProperty("salsify:status")]
        public string? SalsifyStatus { get; set; }

        [JsonProperty("salsify:asset_height")]
        public int? SalsifyAssetHeight { get; set; }

        [JsonProperty("salsify:asset_width")]
        public int? SalsifyAssetWidth { get; set; }

        [JsonProperty("salsify:asset_resource_type")]
        public string? SalsifyAssetResourceType { get; set; }

        [JsonProperty("salsify:filename")]
        public string? SalsifyFilename { get; set; }

        [JsonProperty("salsify:bytes")]
        public int? SalsifyBytes { get; set; }

        [JsonProperty("salsify:format")]
        public string? SalsifyFormat { get; set; }

        [JsonProperty("salsify:etag")]
        public string? SalsifyEtag { get; set; }

        [JsonProperty("salsify:system_id")]
        public string? SalsifySystemId { get; set; }
    }

    public class SalsifyExportHeader
    {
        public string? version { get; set; }
        public List<string>? scope { get; set; }
    }

    public class SalsifyExportIncludedProduct
    {
        [JsonProperty("salsify:product_id")]
        public string? SalsifyProductId { get; set; }

        [JsonProperty("salsify:quantity")]
        public int? SalsifyQuantity { get; set; }
    }

    public class SalsifyExportProduct
    {
        public string ProductID { get; set; } = String.Empty;

        [JsonProperty("salsify:data_inheritance_hierarchy_level_id")]
        public string? SalsifyDataInheritanceHierarchyLevelId { get; set; }

        [JsonProperty("salsify:parent_id")]
        public string? ParentId { get; set; }

        [JsonProperty("salsify:created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("salsify:updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("salsify:id")]
        public string? SalsifyId { get; set; }

        [JsonProperty("Shopify Product ID")]
        public string ShopifyProductId { get; set; }

        [JsonProperty("Shopify Product Hash")]
        public string ShopifyProductHash { get; set; }

        [JsonProperty("Today Date")]
        public string? TodayDate { get; set; }

        [JsonExtensionData]
        public IDictionary<string,JToken> Attributes { get; set; } = new Dictionary<string,JToken>();
    }

    public class SalsifyJsonExport
    {
        public SalsifyExportHeader? header { get; set; }
        public List<SalsifyExportAttribute>? attributes { get; set; }
        public List<SalsifyExportAttributeValue>? attribute_values { get; set; }
        public List<SalsifyExportDigitalAsset>? digital_assets { get; set; }
        public List<SalsifyExportProduct>? products { get; set; }
    }


}

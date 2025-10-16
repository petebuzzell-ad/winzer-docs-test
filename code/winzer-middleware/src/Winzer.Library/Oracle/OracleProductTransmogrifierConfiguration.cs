using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winzer.Library.Salsify
{
    public class OracleProductTransmogrifierConfiguration
    {
        public IDictionary<string, string> ProductMetafieldMapping { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> VariantMetafieldMapping { get; set; } = new Dictionary<string, string>();

        public string ShopifyMetafieldNamespace { get; set; } = "cql";
        public string ProductIDFieldName { get; set; } = "ProductID";
        public string ProductNameFieldName { get; set; } = "Product Name";
        public string ProductTypeFieldName { get; set; } = "Product Type";
        public string CategoryFieldName { get; set; } = "Category";
        public string ProductDescriptionFieldName { get; set; } = "Design Description";
        public string HandbagCollectionFieldName { get; set; } = "Handbag Collection";
        public string JewelryCollectionFieldName { get; set; } = "Jewelry Collection";
        public string GiftWrapFieldName { get; set; } = "GiftWrap";
        public string ProductImageFieldName { get; set; } = "Style Images";
        //public string SendToShopifyFieldName { get; set; } = "Send To Shopify";
        public string SKUFieldName { get; set; } = "ProductID";
        public string PriceFieldName { get; set; } = "Price";
        public string ColorFieldName { get; set; } = "Color";
        public string SwatchImageFieldName { get; set; } = "Swatch Image";
        public string IsTaxableFieldName { get; set; } = "Is Taxable";

        public string TaxCodeFieldName { get; set; } = "Tax Code";
        public string WeightFieldName { get; set; } = "Weight";
        public string DefaultWeightUnit { get; set; } = "POUNDS";

        public string ShopifyTemplateFieldName { get; set; } = "Shopify Template";

        public string ShopifyTagsFieldName { get; set; } = "Shopify Tags";
        public string ShopifyHandleFieldName { get; set; } = "Shopify Handle";
        public string StyleNumberFieldName { get; set; } = "Style Number";
        public string InventoryLifecycleFieldName { get; set; } = "Inventory Lifecycle Code";
        public string NewProductFieldName { get; set; } = "New Product";

        public string InStockFieldName { get; set; } = "In Stock";

        public string DisplayOrderProperty { get; set; } = "Shopify Display Order";

        public IList<string> OptionFieldNames { get; set; } = new List<string>();

        public IDictionary<string, List<string>> SizeSortDictionary { get; set; } = new Dictionary<string, List<string>>();
    }
}

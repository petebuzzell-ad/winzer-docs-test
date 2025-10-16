using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using CsvHelper.Configuration.Attributes;
using System.Reflection;

namespace Winzer.Core.Types
{
    public class OracleCSVRecord
    {
        public string ID { get; set; }
        public string PARENT_PRODUCT_ID { get; set; }
        public string PRIMARY_ITEM_NUMBER { get; set; }
        public string ALTERNATE_ITEM_NUMBERS { get; set; }
        public string OPTION1_NAME { get; set; }
        public string OPTION2_NAME { get; set; }
        public string OPTION3_NAME { get; set; }
        public string OPTION1_VALUE { get; set; }
        public string OPTION2_VALUE { get; set; }
        public string OPTION3_VALUE { get; set; }
        public string PRODUCT_NAME { get; set; }
        public string PRODUCT_DESCRIPTION { get; set; }
        public string SORT_ORDER { get; set; }
        public string IMAGE_URL { get; set; }
        public string ADDITIONAL_IMAGE_URLS { get; set; }
        public string WEBSITE_CATEGORY { get; set; }
        public string METRIC_VERSION { get; set; }
        public string IMPERIAL_VERSION { get; set; }
        public string UOM { get; set; }
        public string VENDOR_NAME { get; set; }
        public string WARNING_BADGES { get; set; }
        public string PRODUCT_BADGE { get; set; }
        public string MINIMUM_ORDER_QUANTITTY { get; set; }
        public string FEATURES { get; set; }
        public string DOCUMENTS { get; set; }
        public string ATTTRIBUTE_NAMES { get; set; }
        public string ATTRIBUTE_VALUES { get; set; }
        public string PACKAGE_DISPLAY { get; set; }
        public string STATUS { get; set; }
        public string ISTAXABLE { get; set; }
        public string TAXCODE { get; set; }
        public string MINIMUM_ORDER_INCREMENT { get; set; }
        public string LEAD_TIME { get; set; }
        public string LOW_INVENTORY_QUANTITY { get; set; }
        public string PROMO_MESSAGING { get; set; }
        public string WEIGHT { get; set; }
        public string WEIGHT_UNIT { get; set; }

        [Ignore]
        public List<OracleCSVRecord> Variants { get; set; }

        [Ignore]
        public string? ShopifyID {  get; set; }
        [JsonIgnore]
        [Ignore]
        public string? ShopifyHash { get; set; }

        public string GetHash()
        {
            using var sha = SHA256.Create();
            string json = JsonConvert.SerializeObject(this);
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
            var sb = new StringBuilder();
            foreach (var byt in hash)
            {
                sb.Append(byt.ToString("x2"));
            };

            return sb.ToString();
        }
        public string? GetPropertyValueAsString(string propertyName)
        {
            Type type = this.GetType();
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{propertyName}' does not exist on type '{type.Name}'.");
            }
            object value = propertyInfo.GetValue(this);
            return value?.ToString();
        }
    }
}

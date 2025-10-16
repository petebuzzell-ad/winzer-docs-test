namespace Cql.Middleware.Library.Oracle
{
    public class PricingCSVRecord
    {
        public string PRODUCT_ID { get; set; }
        public string VARIANT_ID { get; set; }
        public string COMPANY_ID { get; set; }
        public string COMPANY_LOCATION_ID { get; set; }
        public string QUANTITY { get; set; }
        public string CONTRACT_PRICE { get; set; }
        public string LAST_PURCHASE_PRICE { get; set; }
        public string TEMPLATE_NAME { get; set; }
        public string TEMPLATE_PRICE { get; set; }
    }
}

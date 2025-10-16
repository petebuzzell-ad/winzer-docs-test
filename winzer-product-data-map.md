# Winzer Product Data Map - Oracle PIM to Shopify
## Table of Contents
- [Winzer Product Data Map - Oracle PIM to Shopify](#winzer-product-data-map---oracle-pim-to-shopify)
  - [Table of Contents](#table-of-contents)
  - [Overview](#overview)
  - [Data Flow Architecture](#data-flow-architecture)
  - [Oracle PIM Fields](#oracle-pim-fields)
  - [Middleware Transformations](#middleware-transformations)
    - [Data Type Conversions](#data-type-conversions)
    - [Special Processing Logic](#special-processing-logic)
  - [Shopify Metafields](#shopify-metafields)
    - [Product-Level Metafields](#product-level-metafields)
    - [Variant-Level Metafields](#variant-level-metafields)
  - [Special Logic \& Business Rules](#special-logic--business-rules)
    - [Pricing Logic](#pricing-logic)
    - [Variant Processing](#variant-processing)
    - [Image Processing](#image-processing)
    - [Category Processing](#category-processing)
    - [Attribute Processing](#attribute-processing)
  - [Data Quality Considerations](#data-quality-considerations)
    - [Common Data Issues](#common-data-issues)
    - [Error Handling](#error-handling)
## Overview
This document maps the complete data flow from Oracle PIM (Product Information Management) through the AWS Middleware system to Shopify metafields. The middleware system performs critical transformations and data enrichment during this process.
**Data Source:** Oracle ERP exports CSV files via SFTP to AWS S3, which are then processed by Lambda functions and transformed into Shopify products with custom metafields.
## Data Flow Architecture

```
Oracle PIM → CSV Export → SFTP Transfer → AWS S3 → Lambda Processing → Shopify API
```

**Process Flow:**
1. **Oracle PIM** exports product data as CSV files
2. **SFTP Transfer** moves files to AWS S3 storage
3. **Lambda Processing** transforms and validates data
4. **Shopify API** creates/updates products and variants
## Oracle PIM Fields
The following table shows all Oracle PIM fields from the CSV export and their corresponding transformations:

| Oracle PIM Field                                        | Middleware Processing                                | Shopify Destination                                | Data Type                    | Required    | Notes                                                  |
| ------------------------------------------------------- | ---------------------------------------------------- | -------------------------------------------------- | ---------------------------- | ----------- | ------------------------------------------------------ |
| `ID`                                                    | Used for product identification and variant grouping | Product ID (if updating existing)                  | String                       | Yes         | Unique product identifier from Oracle                  |
| `PARENT_PRODUCT_ID`                                     | Used to group variants under parent products         | Product grouping logic                             | String                       | No          | Determines product hierarchy and variant relationships |
| `PRIMARY_ITEM_NUMBER`                                   | Direct mapping to SKU                                | Variant SKU                                        | String                       | Yes         | Primary identifier for the variant                     |
| `ALTERNATE_ITEM_NUMBERS`                                | Mapped to metafield                                  | `cql.alternate_item_numbers`                       | String                       | No          | Additional SKU references                              |
| `OPTION1_NAME`<br/>`OPTION2_NAME`<br/>`OPTION3_NAME`    | Used to create Shopify product options               | Product Options (Size, Color, etc.)                | String                       | No          | Defines variant option names                           |
| `OPTION1_VALUE`<br/>`OPTION2_VALUE`<br/>`OPTION3_VALUE` | Mapped to variant option values                      | Variant Option Values                              | String                       | No          | Specific values for each option                        |
| `PRODUCT_NAME`                                          | Direct mapping                                       | Product Title                                      | String                       | Yes         | Main product name                                      |
| `PRODUCT_DESCRIPTION`                                   | Direct mapping with HTML processing                  | Product Description (HTML)                         | HTML                         | No          | Rich text product description                          |
| `SORT_ORDER`                                            | Used for variant ordering                            | Variant Position                                   | Integer                      | No          | Determines variant display order                       |
| `IMAGE_URL`                                             | Aggregated with additional images                    | Product Images                                     | URL                          | No          | Primary product image                                  |
| `ADDITIONAL_IMAGE_URLS`                                 | Split by comma and aggregated                        | Additional Product Images                          | URL Array                    | No          | Comma-separated list of additional images              |
| `WEBSITE_CATEGORY`                                      | Split by '>' and converted to JSON array             | `cql.categories` (list.single_line_text_field)     | JSON Array                   | No          | Hierarchical category structure                        |
| `METRIC_VERSION`                                        | Mapped to metafield                                  | `cql.metric_version` (product_reference)           | Product Reference            | No          | Reference to metric version product                    |
| `IMPERIAL_VERSION`                                      | Mapped to metafield                                  | `cql.imperial_version` (product_reference)         | Product Reference            | No          | Reference to imperial version product                  |
| `UOM`                                                   | Mapped to variant metafield                          | `cql.winzer_uom`                                   | String                       | No          | Unit of measure                                        |
| `VENDOR_NAME`                                           | Mapped to metafield                                  | `cql.vendor_name`                                  | String                       | No          | Brand/manufacturer name                                |
| `WARNING_BADGES`                                        | Split by comma and converted to JSON array           | `cql.warning_badges` (list.single_line_text_field) | JSON Array                   | No          | Safety warning badges                                  |
| `PRODUCT_BADGE`                                         | Split by comma and converted to JSON array           | `cql.product_badge` (list.single_line_text_field)  | JSON Array                   | No          | Product feature badges                                 |
| `MINIMUM_ORDER_QUANTITTY`                               | Mapped to variant metafield as integer               | `cql.minimum_order_quantity` (number_integer)      | Integer                      | No          | Minimum order quantity for variant                     |
| `FEATURES`                                              | Mapped to metafield                                  | `cql.features_text`                                | String                       | No          | Product features and specifications                    |
| `DOCUMENTS`                                             | Mapped to metafield                                  | `cql.documents`                                    | String                       | No          | Document links (SDS, manuals, etc.)                    |
| `ATTTRIBUTE_NAMES`<br/>`ATTRIBUTE_VALUES`               | Split by '                                           | ' and combined into JSON object                    | `cql.attributes_json` (json) | JSON Object | No                                                     | Dynamic product attributes for SearchSpring filtering |
| `PACKAGE_DISPLAY`                                       | Mapped to variant metafield                          | `cql.package_display`                              | String                       | No          | Package display information                            |
| `STATUS`                                                | Converted to uppercase                               | Product Status                                     | String                       | Yes         | Product availability status                            |
| `ISTAXABLE`                                             | Converted to boolean (defaults to true)              | Variant Taxable                                    | Boolean                      | No          | Tax calculation setting                                |
| `TAXCODE`                                               | Direct mapping                                       | Variant Tax Code                                   | String                       | No          | Tax code for variant                                   |
| `MINIMUM_ORDER_INCREMENT`                               | Not currently mapped                                 | -                                                  | String                       | No          | Available for future use                               |
| `LEAD_TIME`                                             | Mapped to variant metafield as integer               | `cql.product_lead_time` (number_integer)           | Integer                      | No          | Product lead time in days                              |
| `LOW_INVENTORY_QUANTITY`                                | Mapped to variant metafield as integer               | `cql.low_inventory_quantity` (number_integer)      | Integer                      | No          | Low stock threshold                                    |
| `PROMO_MESSAGING`                                       | Mapped to variant metafield                          | `cql.promo_messaging`                              | String                       | No          | Promotional messaging for variant                      |
| `WEIGHT`                                                | Converted to decimal with unit handling              | Variant Weight                                     | Decimal                      | No          | Product weight for shipping                            |
| `WEIGHT_UNIT`                                           | Converted to Shopify format (LB→POUNDS, OZ→OUNCES)   | Variant Weight Unit                                | String                       | No          | Weight unit for shipping calculations                  |
## Middleware Transformations
The AWS Lambda middleware performs several critical transformations during the data processing:
### Data Type Conversions
- **String to Boolean:** `ISTAXABLE` field converted to boolean (defaults to true if empty)
- **String to Integer:** `MINIMUM_ORDER_QUANTITTY`, `LEAD_TIME`, `LOW_INVENTORY_QUANTITY` converted to integers
- **String to Decimal:** `WEIGHT` field converted to decimal for shipping calculations
- **String to JSON Array:** `WEBSITE_CATEGORY` split by '>' and converted to JSON array
- **String to JSON Object:** `ATTTRIBUTE_NAMES` and `ATTRIBUTE_VALUES` combined into key-value JSON object
### Special Processing Logic
- **Variant Grouping:** Products with `PARENT_PRODUCT_ID` are grouped as variants under the parent product
- **Image Aggregation:** Primary and additional images are collected from all variants and assigned to the product level
- **Option Creation:** `OPTION1_NAME`, `OPTION2_NAME`, `OPTION3_NAME` create Shopify product options
- **Duplicate Prevention:** Variants with identical option combinations are filtered out
- **Variant Limiting:** Maximum 250 variants per product (first 100 if exceeded)
## Shopify Metafields
All custom data is stored in Shopify metafields with the namespace `cql`:
### Product-Level Metafields

| Metafield Key          | Type                        | Source Field     | Purpose                               |
| ---------------------- | --------------------------- | ---------------- | ------------------------------------- |
| `cql.categories`       | list.single_line_text_field | WEBSITE_CATEGORY | SearchSpring filtering and navigation |
| `cql.metric_version`   | product_reference           | METRIC_VERSION   | Reference to metric version product   |
| `cql.imperial_version` | product_reference           | IMPERIAL_VERSION | Reference to imperial version product |
| `cql.vendor_name`      | single_line_text_field      | VENDOR_NAME      | Brand identification                  |
| `cql.product_badge`    | list.single_line_text_field | PRODUCT_BADGE    | Product feature badges                |
| `cql.features_text`    | single_line_text_field      | FEATURES         | Product features and specifications   |
| `cql.documents`        | single_line_text_field      | DOCUMENTS        | Document links (SDS, manuals, etc.)   |

### Variant-Level Metafields

| Metafield Key                | Type                        | Source Field                        | Purpose                                       |
| ---------------------------- | --------------------------- | ----------------------------------- | --------------------------------------------- |
| `cql.alternate_item_numbers` | single_line_text_field      | ALTERNATE_ITEM_NUMBERS              | Additional SKU references                     |
| `cql.variant_description`    | multi_line_text_field       | PRODUCT_DESCRIPTION                 | Variant-specific description                  |
| `cql.winzer_uom`             | single_line_text_field      | UOM                                 | Unit of measure                               |
| `cql.minimum_order_quantity` | number_integer              | MINIMUM_ORDER_QUANTITTY             | Minimum order quantity                        |
| `cql.package_display`        | single_line_text_field      | PACKAGE_DISPLAY                     | Package display information                   |
| `cql.warning_badges`         | list.single_line_text_field | WARNING_BADGES                      | Safety warning badges                         |
| `cql.product_lead_time`      | number_integer              | LEAD_TIME                           | Product lead time in days                     |
| `cql.low_inventory_quantity` | number_integer              | LOW_INVENTORY_QUANTITY              | Low stock threshold                           |
| `cql.promo_messaging`        | single_line_text_field      | PROMO_MESSAGING                     | Promotional messaging                         |
| `cql.attributes_json`        | json                        | ATTTRIBUTE_NAMES + ATTRIBUTE_VALUES | Dynamic attributes for SearchSpring filtering |
## Special Logic & Business Rules
### Pricing Logic
**Default Pricing:** Winzer and FastServ products are set to $1,000,000 by default to allow catalog pricing to fall back as expected. This enables the B2B pricing system to override with actual customer-specific pricing.
### Variant Processing
**Variant Validation:** Variants missing required option values are skipped with warning logs. Duplicate variants (same option combination) are filtered out.
### Image Processing
**Image Aggregation:** All images from variants are collected and assigned to the product level. Maximum 250 images per product. Image filenames are stored in the Alt text field for matching purposes.
### Category Processing
**Hierarchical Categories:** `WEBSITE_CATEGORY` is split by '>' and converted to a JSON array for SearchSpring filtering. Example: "Abrasives>Brushes>Wire Wheels & Brushes" becomes ["Abrasives", "Brushes", "Wire Wheels & Brushes"].
### Attribute Processing
**Dynamic Attributes:** `ATTTRIBUTE_NAMES` and `ATTRIBUTE_VALUES` are split by '|' and combined into a JSON object for SearchSpring filtering. This enables dynamic product filtering based on technical specifications.
## Data Quality Considerations
### Common Data Issues
- **Missing Required Fields:** Some products may lack essential data like `PRIMARY_ITEM_NUMBER` or `PRODUCT_NAME`
- **Format Inconsistencies:** Weight units, measurements, and other fields may have inconsistent formatting
- **HTML Content:** Product descriptions may contain malformed HTML that needs cleaning
- **Image URLs:** Some image URLs may be broken or inaccessible
- **Attribute Mismatches:** `ATTTRIBUTE_NAMES` and `ATTRIBUTE_VALUES` arrays may have different lengths
### Error Handling
- **Validation Errors:** Products with critical missing data are logged and skipped
- **Type Conversion Errors:** Invalid numeric values are handled gracefully with defaults
- **Duplicate Prevention:** Duplicate variants are logged and filtered out
- **Size Limits:** Products exceeding variant limits are truncated with warnings
---
**Document Version:** 1.0
**Last Updated:** September 24, 2025
**Next Review:** December 24, 2025
**Document Owner:** Winzer eCommerce Team
**Related Documentation:** [AWS Middleware Documentation](winzer-middleware-documentation.html) | [Main Platform Documentation](winzer-documentation.html)
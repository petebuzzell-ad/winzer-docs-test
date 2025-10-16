# ShipperHQ Shipping Rules Documentation

## Table of Contents

1. [Shipping Rules Overview](#shipping-rules-overview)
2. [Shipping Groups Metafield](#shipping-groups-metafield)
3. [Shipping Group Values](#shipping-group-values)
4. [Shipping Rules Logic](#shipping-rules-logic)
5. [ShipperHQ Integration](#shipperhq-integration)
6. [Customer Experience](#customer-experience)
7. [Data Flow](#data-flow)
8. [Maintenance & Troubleshooting](#maintenance--troubleshooting)

---

## Shipping Rules Overview

ShipperHQ is integrated with all three Winzer eCommerce sites to handle complex shipping rules that are product-based. The system uses variant-level metafields to control shipping restrictions, carrier limitations, and state-specific shipping rules.

### Key Features

- **Product-Based Rules:** Shipping restrictions are applied at the variant level
- **State Restrictions:** Products can be restricted from shipping to specific states
- **Hazardous Material Handling:** Special rules for hazmat products and carrier limitations
- **Size Classifications:** Products categorized by size for shipping method availability
- **Dynamic Checkout:** Shipping options update based on cart contents and restrictions

> **Integration Note:** ShipperHQ reads shipping restriction data from Shopify variant metafields and automatically applies rules during the checkout process to determine available shipping options.

## Shipping Groups Metafield

The core of the shipping rules system is the `global.SHIPPING_GROUPS` variant metafield, which contains all shipping restriction identifiers for each product variant.

### Metafield Configuration

| Property       | Value                    | Description                                            |
| -------------- | ------------------------ | ------------------------------------------------------ |
| **Field Name** | `global.SHIPPING_GROUPS` | Variant-level metafield identifier                     |
| **Data Type**  | String                   | Hash-delimited list of shipping restriction values     |
| **Scope**      | Variant                  | Applied to individual product variants, not products   |
| **Required**   | No                       | Only populated for products with shipping restrictions |

### Data Format

The metafield contains a hash-delimited string of shipping restriction identifiers. ShipperHQ expects the `#` delimiter to separate different restriction values:

```javascript
// Example shipping groups values (hash-delimited format)
"SMALL#CA#NY"                    // Small product restricted from CA and NY
"Hazardous Products#SMALL"       // Hazmat small product
"LARGE#TX#FL"                    // Large product restricted from TX and FL
"MEDIUM"                         // Medium product with no restrictions
""                               // No shipping restrictions

// Real example from SKU J91.4004
"CA#Hazardous Products#MEDIUM"   // Hazmat medium product restricted from CA

// Multiple restrictions example
"Hazardous Products#SMALL#CA#NY#TX"  // Hazmat small product restricted from CA, NY, and TX
```

> **Critical Format Note:** ShipperHQ requires the hash delimiter (`#`) to properly parse shipping restriction values. Using commas or other delimiters will cause the restrictions to not be recognized.

## Shipping Group Values

The shipping groups system uses several types of identifiers to control shipping behavior:

### Size Classifications

| Value    | Description                   | Usage                                                   |
| -------- | ----------------------------- | ------------------------------------------------------- |
| `SMALL`  | Small package classification  | Determines available shipping methods and pricing tiers |
| `MEDIUM` | Medium package classification | Standard shipping methods and pricing                   |
| `LARGE`  | Large package classification  | Limited shipping methods, may require special handling  |

### Hazardous Material Classification

| Value                | Description                             | Shipping Restrictions                             |
| -------------------- | --------------------------------------- | ------------------------------------------------- |
| `Hazardous Products` | Products containing hazardous materials | Cannot ship via UPS Next Day Air or UPS 2 Day Air |

### State Restrictions

Products can be restricted from shipping to specific states using state abbreviation codes. When used in the SHIPPING_GROUPS metafield, state codes must be separated by the hash delimiter:

```javascript
// Individual state codes (for reference)
"CA"    // California
"NY"    // New York  
"TX"    // Texas
"FL"    // Florida
"IL"    // Illinois
"PA"    // Pennsylvania
// ... all 50 state abbreviations supported

// State restrictions in SHIPPING_GROUPS metafield (hash-delimited)
"CA#NY#TX"                       // Restricted from CA, NY, and TX
"FL#IL"                          // Restricted from FL and IL
"CA"                             // Restricted from CA only
"SMALL#CA#NY"                    // Small product restricted from CA and NY
```

## Shipping Rules Logic

ShipperHQ processes the shipping groups data to apply specific shipping rules during checkout:

### State Restriction Logic

```javascript
// State restriction processing (ShipperHQ parses hash-delimited data)
shipping_groups = product.shipping_groups.split("#")
restricted_states = shipping_groups.filter(group => isStateCode(group))

if (restricted_states.contains(customer_state)) {
    // Remove shipping options for this state
    available_shipping_methods = filter_by_excluded_states(customer_state)
}

// Example: "SMALL#CA#NY" becomes ["SMALL", "CA", "NY"]
// ShipperHQ identifies "CA" and "NY" as state restrictions
```

### Hazardous Material Logic

```javascript
// Hazardous material processing (ShipperHQ parses hash-delimited data)
shipping_groups = product.shipping_groups.split("#")

if (shipping_groups.contains("Hazardous Products")) {
    // Exclude restricted shipping methods
    available_shipping_methods = exclude_methods([
        "UPS Next Day Air",
        "UPS 2 Day Air"
    ])
}

// Example: "Hazardous Products#SMALL#CA" 
// ShipperHQ identifies "Hazardous Products" and applies hazmat restrictions
```

### Size Classification Logic

```javascript
// Size-based shipping method filtering (ShipperHQ parses hash-delimited data)
shipping_groups = product.shipping_groups.split("#")
size_classification = shipping_groups.find(group => ["SMALL", "MEDIUM", "LARGE"].contains(group))

switch (size_classification) {
    case "SMALL":
        available_methods = get_small_package_methods()
        break
    case "MEDIUM":
        available_methods = get_standard_methods()
        break
    case "LARGE":
        available_methods = get_large_package_methods()
        break
}

// Example: "LARGE#TX#FL" 
// ShipperHQ identifies "LARGE" and applies large package shipping rules
```

### Real-World Examples

Here are practical examples of how the hash-delimited SHIPPING_GROUPS metafield works in different scenarios:

| Product Scenario                               | SHIPPING_GROUPS Value              | Shipping Behavior                                                                                  |
| ---------------------------------------------- | ---------------------------------- | -------------------------------------------------------------------------------------------------- |
| Small hazmat product restricted from CA and NY | `"Hazardous Products#SMALL#CA#NY"` | Cannot ship via UPS Next Day Air or 2 Day Air; cannot ship to CA or NY; uses small package methods |
| Large product with no restrictions             | `"LARGE"`                          | Uses large package shipping methods; no state or carrier restrictions                              |
| Medium product restricted from multiple states | `"MEDIUM#TX#FL#IL#PA"`             | Uses standard shipping methods; cannot ship to TX, FL, IL, or PA                                   |
| Hazmat product with state restrictions         | `"Hazardous Products#CA#NY#TX"`    | Cannot ship via UPS Next Day Air or 2 Day Air; cannot ship to CA, NY, or TX                        |
| Small product with single state restriction    | `"SMALL#CA"`                       | Uses small package methods; cannot ship to California only                                         |
| **Real Example (SKU J91.4004)**                | `"CA#Hazardous Products#MEDIUM"`   | Hazmat medium product restricted from California; cannot ship via UPS Next Day Air or 2 Day Air    |
| Product with no shipping restrictions          | `""` or empty                      | All standard shipping methods available; no restrictions                                           |

## ShipperHQ Integration

ShipperHQ integrates with Shopify to read shipping restriction data and apply rules during the checkout process:

### Data Synchronization

- **Real-time Updates:** ShipperHQ reads metafield data during checkout
- **Cart Evaluation:** All products in cart are evaluated for restrictions
- **Dynamic Filtering:** Available shipping methods update based on cart contents

### Integration Points

| Integration Point | Trigger                  | Action                                         |
| ----------------- | ------------------------ | ---------------------------------------------- |
| **Product Page**  | Product added to cart    | Shipping restrictions noted for checkout       |
| **Cart Page**     | Cart contents change     | Shipping options updated based on restrictions |
| **Checkout**      | Shipping address entered | Final shipping method filtering applied        |

## Customer Experience

The shipping rules system provides a seamless experience while ensuring compliance with shipping restrictions:

### Restriction Communication

- **Transparent Messaging:** Customers see clear information about shipping limitations
- **Alternative Options:** Available shipping methods are clearly presented
- **State-Specific Warnings:** Customers are informed when products cannot ship to their state

### Checkout Flow

1. **Cart Review:** Customer reviews products and shipping restrictions
2. **Address Entry:** Customer enters shipping address
3. **Method Selection:** Available shipping methods are displayed
4. **Restriction Handling:** Restricted methods are automatically excluded
5. **Order Completion:** Order processes with compliant shipping method

## Data Flow

The shipping restriction data flows through the system as follows:

### Data Source to Checkout

```javascript
// Data flow for shipping restrictions
Oracle PIM → Middleware → Shopify Metafields → ShipperHQ → Checkout

1. Oracle PIM: Product data with shipping classifications
2. Middleware: Processes and maps shipping data to metafields
3. Shopify: Stores global.SHIPPING_GROUPS metafield data
4. ShipperHQ: Reads metafield data during checkout
5. Checkout: Applies rules and filters shipping options
```

### Middleware Processing

The AWS middleware system processes shipping restriction data from Oracle PIM and maps it to the appropriate Shopify metafields:

- **Data Mapping:** Oracle shipping classifications mapped to SHIPPING_GROUPS values
- **State Code Processing:** State restrictions converted to standard abbreviations
- **Hazmat Identification:** Hazardous material flags converted to "Hazardous Products"
- **Size Classification:** Product dimensions converted to SMALL/MEDIUM/LARGE

## Maintenance & Troubleshooting

This section covers ongoing maintenance and troubleshooting for the ShipperHQ shipping rules system:

### Common Issues & Solutions

| Issue                                           | Cause                                         | Solution                                                       |
| ----------------------------------------------- | --------------------------------------------- | -------------------------------------------------------------- |
| Shipping restrictions not applying              | Metafield data not synced or incorrect format | Verify global.SHIPPING_GROUPS metafield data and format        |
| State restrictions not working                  | Incorrect state abbreviation codes            | Verify state codes match standard abbreviations (CA, NY, etc.) |
| Hazmat products shipping via restricted methods | Missing "Hazardous Products" identifier       | Add "Hazardous Products" to SHIPPING_GROUPS metafield          |
| Size classifications not affecting shipping     | Missing or incorrect size values              | Verify SMALL/MEDIUM/LARGE values in metafield                  |

### Testing Shipping Rules

1. **Test State Restrictions:** Create test orders with restricted state addresses
2. **Test Hazmat Products:** Verify hazmat products exclude restricted shipping methods
3. **Test Size Classifications:** Confirm size-based shipping method availability
4. **Test Combined Rules:** Test products with multiple restriction types

### Monitoring & Maintenance

- **Regular Audits:** Review shipping restriction data for accuracy
- **Rule Updates:** Update restrictions as regulations change
- **Performance Monitoring:** Monitor checkout completion rates and shipping issues
- **Customer Feedback:** Track customer complaints related to shipping restrictions

---

## Questions or Issues?

For technical questions about this ShipperHQ implementation, contact:

**Arcadia Digital**  
[pete.buzzell@arcadiadigital.com](mailto:pete.buzzell@arcadiadigital.com)

## Additional Resources

For general ShipperHQ questions and documentation:

- **ShipperHQ Documentation:** [https://help.shipperhq.com/](https://help.shipperhq.com/)
- **ShipperHQ API Reference:** [https://docs.shipperhq.com/](https://docs.shipperhq.com/)

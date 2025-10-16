# SearchSpring Custom Implementation Documentation

## Table of Contents

1. [Custom Implementation Overview](#custom-implementation-overview)
2. [Site-Specific Configurations](#site-specific-configurations)
3. [Custom Filter Components](#custom-filter-components)
4. [Image Filter Implementations](#image-filter-implementations)
5. [Custom Pricing Logic](#custom-pricing-logic)
6. [Custom Styling & Branding](#custom-styling-branding)
7. [Dynamic Variant Handling](#dynamic-variant-handling)
8. [Metafield Integrations](#metafield-integrations)
9. [Custom Search Behaviors](#custom-search-behaviors)
10. [Technical Maintenance](#technical-maintenance)

---

## Custom Implementation Overview

This document details the custom SearchSpring implementations built specifically for the Winzer eCommerce ecosystem. The implementation includes site-specific configurations, custom components, and brand-specific functionality across three distinct stores.

### Implementation Architecture

- **Multi-Site Configuration:** Single codebase supporting three distinct brand experiences
- **Custom Components:** Branded filter displays, pricing logic, and variant handling
- **Site-Specific Features:** Different pricing models, authentication requirements, and styling per brand
- **Custom Metafield Integration:** Advanced filtering using structured product attributes
- **Dynamic Variant Selection:** Custom variant handling with image swapping and pricing

> **Technical Note:** The implementation uses a single SearchSpring codebase with site-specific configurations determined by `window.Resources.searchspring.liquid_data.sitename` to deliver customized experiences for each brand.

## Site-Specific Configurations

The implementation uses site-specific feature flags to deliver different functionality across the three Winzer brands:

### Site Feature Configuration

| Feature           | OneSource  | FastServ   | Winzer Corp | Description                             |
| ----------------- | ---------- | ---------- | ----------- | --------------------------------------- |
| **Bulk Pricing**  | ✅ Enabled  | ❌ Disabled | ❌ Disabled  | Volume-based pricing calculations       |
| **Force Sign In** | ❌ Disabled | ✅ Enabled  | ✅ Enabled   | Requires authentication to view pricing |
| **Exact Pricing** | ❌ Disabled | ✅ Enabled  | ✅ Enabled   | Real-time pricing via API calls         |
| **B2B Pricing**   | ❌ Disabled | ✅ Enabled  | ✅ Enabled   | Customer-specific pricing tiers         |

### Site Identification Logic

```javascript
const stores = {
  onesource: {
    storeAddress: "winzeronesource.myshopify.com",
    siteId: "t047mf"
  },
  fastserv: {
    storeAddress: "winzerfastserv.myshopify.com", 
    siteId: "wk4j0d"
  },
  corp: {
    storeAddress: "winzercorp.myshopify.com",
    siteId: "fsqw40"
  }
}
```

## Custom Filter Components

The implementation includes several custom components built specifically for the Winzer brands:

### CustomFacetPaletteOptions

Custom implementation for image-based filters that dynamically constructs image URLs and applies site-specific styling:

```javascript
const CustomFacetPaletteOptions = ({ facet, values }) => {
  const { field } = facet;
  const { label } = facet;
  const shopifyFileURL = window.Resources.searchspring.shopifyURLs.fileURL.split('/placeholder')[0];
  let columns = label.includes('olor') ? 4 : 3;

  const transformedValues = useMemo(() => {
    return values.filter(v => v.label);
  }, [values]);

  const facetCss = useMemo(() => {
    const styles = {};
    transformedValues.forEach(v => {
      let extension = field === "variant_head_style" || field === "ss_generic_head_type" ? ".svg" : ".png";
      let filterLabel = v.label.replaceAll(' ', '-').toLowerCase();
      let filterImage = shopifyFileURL + "/" + field + "__" + filterLabel + extension;
      let background = `url("${filterImage}")`;
      
      styles[`& .ss__facet-palette-options__option__palette--${filterLabel}`] = {
        display: 'block',
        background: background,
      };
    });
    return [css(styles)];
  }, [transformedValues]);

  return <FacetPaletteOptions css={facetCss} values={transformedValues} facet={facet} columns={columns} hideCount={true} hideIcon gapSize='0px' />;
};
```

### CustomFeaturedFilter

Custom carousel component for featured filters with mobile/desktop responsive design:

```javascript
const CustomFeaturedFilter = ({ label, url, imageUrl }) => {
  const isSelected = (window.location.href.includes(label) ? 'selected-filter' : '')
  return <a href={url.href} className={`ss__featured-filter ${isSelected}`}>
    <img src={imageUrl} />
    <span>{label}</span>
  </a>
};
```

## Custom Pricing Logic

The implementation includes sophisticated pricing logic that varies by site and customer authentication status:

### Pricing API Integration

For sites with exact pricing enabled (FastServ, Winzer Corp), the system makes real-time API calls to fetch customer-specific pricing:

```javascript
const fetchB2BPricing = async () => {
  const pricing_json_response = await fetch(`${core.url}?view=pricing_json`);
  const pricing_json = await pricing_json_response.json();
  setb2bPricing(pricing_json);
}

// API call for exact pricing
fetch(`/apps/pricing-api/customer-product-pricing?current_company_id=${companyId}&product_ids=${productId}`, {
  method: 'GET',
  headers: {
    'Content-Type': 'application/json',
  }
})
```

### Pricing Display Logic

```javascript
// Pricing display based on site features
if (currentSiteFeatures.b2b_pricing) {
  // Show B2B pricing with API calls
} else if (currentSiteFeatures.bulk_pricing) {
  // Show bulk pricing calculations
} else {
  // Show standard Shopify pricing
}
```

### Force Sign-In Implementation

For B2B sites, pricing is hidden until user authentication:

```javascript
export const force_sign_in = (currentSiteFeatures.force_sign_in && !signed_in);

// In component render
{force_sign_in ? (
  <span className="not-in-catalog">{plpLocales.not_in_catalog}</span>
) : (
  // Show pricing
)}
```

## Custom Styling & Branding

The implementation includes extensive custom styling with site-specific branding and responsive design:

### Site-Specific CSS Classes

Each site has dedicated CSS classes for brand-specific styling:

```css
/* OneSource specific styling */
.site-winzeronesource .ss__facet-hierarchy-options__option--return:before {
  content: url('data:image/svg+xml,&lt;svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 11 10"&gt;&lt;path stroke="%23db3700" stroke-linecap="round" stroke-width="1.2" d="M5 1 1 5l4 4"/&gt;&lt;path fill="%23db3700" d="M9.535 5.606a.6.6 0 1 0 0-1.2v1.2Zm-8 0h8v-1.2h-8v1.2Z"/&gt;&lt;/svg&gt;')!important;
}
/* Winzer Corp specific styling */
.site-winzercorp .ss__facet-hierarchy-options__option--return:before {
  content: url('data:image/svg+xml,&lt;svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 11 10"&gt;&lt;path stroke="%2300679b" stroke-linecap="round" stroke-width="1.2" d="M5 1 1 5l4 4"/&gt;&lt;path fill="%2300679b" d="M9.535 5.606a.6.6 0 1 0 0-1.2v1.2Zm-8 0h8v-1.2h-8v1.2Z"/&gt;&lt;/svg&gt;')!important;
}
/* FastServ specific styling */
.site-winzerfastserv .ss__facet-hierarchy-options__option--return:before {
  content: url('data:image/svg+xml,&lt;svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 11 10"&gt;&lt;path stroke="%23d32028" stroke-linecap="round" stroke-width="1.2" d="M5 1 1 5l4 4"/&gt;&lt;path fill="%23d32028" d="M9.535 5.606a.6.6 0 1 0 0-1.2v1.2Zm-8 0h8v-1.2h-8v1.2Z"/&gt;&lt;/svg&gt;')!important;
}
```

### Brand Color Implementation

| Brand           | Primary Color | Hex Code | Usage                          |
| --------------- | ------------- | -------- | ------------------------------ |
| **OneSource**   | Orange/Red    | #db3700  | Filter icons, buttons, accents |
| **Winzer Corp** | Blue          | #00679b  | Filter icons, buttons, accents |
| **FastServ**    | Red           | #d32028  | Filter icons, buttons, accents |

## Dynamic Variant Handling

The implementation includes sophisticated variant selection and display logic that responds to user interactions and search filters:

### Dynamic Variant Configuration

```javascript
let dynamicVariantsConfig = {
  field: 'ss_swatches',
  limit: 4,
  swap: function(result, variant) {
    const core = result.mappings.core;
    const { attributes, custom } = result;

    if(variant.image) {
      core.imageUrl = variant.image;
    }

    custom.variantSelected = variant;
  }
};
```

### Variant Weight Calculation

The system calculates variant relevance based on search terms and active filters:

```javascript
// Filter matching for variant weighting
currentFilters.forEach((filter, i) => {
  let filterMultiplier = i + 1;
  
  if (simpleData) {
    variant.weight = calculateWeight(filter, simpleData, variant.weight, 10, filterMultiplier);
  }
  groupData.forEach((data) => {
    variant.weight = calculateWeight(filter, data, variant.weight, 5, filterMultiplier);
  });
});

// Sort variants by weight
attributes[variantsConfig.field].sort((a, b) => {
  return b.weight - a.weight;
});
```

### Variant Image Swapping

When users interact with variant swatches, the main product image updates dynamically:

```javascript
const handleSelectedClass = (e, index) => {
  let swatchIndex = e.target.getAttribute('swatchIndex');
  if(swatchIndex == index) {
    setSelectedSwatch(swatchIndex);
  }
};

const handleSelectedURL = () => {
  const { custom } = result;
  const variantURL = custom.variantSelected.url;
  let variantID = variantURL.slice(-14)
  setSelectedURL(variantID);
}
```

## Metafield Integrations

The implementation leverages multiple Shopify metafields for advanced filtering and product display:

### Product Display Metafields

| Metafield                    | Usage                         | Data Type | Example                   |
| ---------------------------- | ----------------------------- | --------- | ------------------------- |
| `mfield_cql_badge_label`     | Product badge text            | String    | "New", "Sale", "Featured" |
| `mfield_cql_product_badges`  | JSON array of badges          | JSON      | ["badge1", "badge2"]      |
| `mfield_cql_swatches_json`   | Variant swatch data           | JSON      | Color/pattern data        |
| `mfield_cql_vendor_name`     | Custom vendor display         | String    | Brand name override       |
| `mfield_cql_promo_messaging` | Promotional text              | String    | Special offers, discounts |
| `mfield_cql_attributes_json` | Structured product attributes | JSON      | Filterable product specs  |

### Filter Processing Logic

The system processes filters through multiple data sources with fallback logic:

```javascript
function filterVariants(variants, options, filters) {
  function extractFromAttributes(key, attributesField) {
    if (!attributesField) {
      return false;
    }
    const attributes_json = JSON.parse(attributesField);
    const formattedKey = key.replace('ss_', '').split('_').map(g => g.charAt(0).toUpperCase() + g.slice(1)).join(' ');
    return attributes_json[formattedKey] ?? false;
  }
  
  return variants.filter((v) => {
    const filter_map = filters.reduce((acc, filter) => {
      acc[filter.field] = filter.values.map((val) => val.value);
      return acc;
    }, {});
    
    return Object.keys(filter_map).map((key) => {
      if (key === "ss_price") {
        return filter_map[key].low <= parseFloat(v.price) && parseFloat(v.price) <= filter_map[key].high;
      } else if (v[key]) {
        return v[key].split('|').some((v) => filter_map[key].indexOf(v) > -1);
      } else if (extractFromAttributes(key, v.mfield_cql_attributes_json)) {
        return extractFromAttributes(key, v.mfield_cql_attributes_json).split('|').some((v) => filter_map[key].indexOf(v) > -1);
      } else {
        return key.indexOf('ss_generic') === -1;
      }
    }).every((t) => t);
  });
}
```

## Custom Search Behaviors

The implementation includes several custom search and filtering behaviors tailored to the Winzer product catalog:

### Sorting Logic

```javascript
function sortVariants(variants, sortParams) {
  let sortedVariants = variants.sort((a, b) => {
    const sortableAttrA = a[sortParams.option].toLowerCase();
    const sortableAttrB = b[sortParams.option].toLowerCase();
    
    if (!isNaN(sortableAttrA) && !isNaN(sortableAttrA)) {
      return parseFloat(sortableAttrA) - parseFloat(sortableAttrB);
    }
    return sortableAttrA > sortableAttrB ? 1 : -1;
  });
  
  if (!sortParams.asc) sortedVariants.reverse();
  return sortedVariants;
}
```

### Filter Visibility Rules

- **Minimum Product Count:** Filters only appear when 3+ products match
- **Value Variation:** Filters hidden when all values are identical
- **Site-Specific Logic:** Different visibility rules per brand
- **Dynamic Updates:** Filters update in real-time as search terms change

### Mobile Optimization

```javascript
// Mobile-specific filter display
const { width, height, valuesWithImages } = useMemo(() => {
  const width = isMobile ? 110 : 120
  const height = isMobile ? 72 : 80
  const valuesWithImages = values.filter(value => !!getImageObj(facet.field, value.value))
  return { width, height, valuesWithImages }
}, [facet.field, values, isMobile])
```

## Technical Maintenance

This section covers ongoing maintenance and troubleshooting for the custom SearchSpring implementation:

### Data Synchronization

- **Daily Reindexing:** SearchSpring automatically rebuilds its index daily
- **Manual Reindex:** Available through SearchSpring dashboard when needed.
  - Takes anywhere from 10 - 30 minutes to complete.
  - Once index has rebuilt, it takes ~5 minutes for changes to appear on the Shopify storefront.
- **Metafield Updates:** Changes to Shopify metafields require reindexing to appear in filters
- **Product Updates:** New products and variants are included in next scheduled reindex

### Common Issues & Solutions

| Issue                         | Cause                                   | Solution                                           |
| ----------------------------- | --------------------------------------- | -------------------------------------------------- |
| Filter images not displaying  | Incorrect file naming or missing files  | Verify naming convention: `[field]__[value].[ext]` |
| Filters not appearing         | Insufficient product variation          | Ensure 3+ products with different values           |
| Pricing not updating          | API connection or authentication issues | Check B2B pricing API status and credentials       |
| Variant selection not working | JavaScript errors or missing metafields | Verify `ss_swatches` metafield data structure      |

### Performance Monitoring

- **SearchSpring Dashboard:** Monitor search performance and filter usage
- **Google Analytics:** Track user interactions with filters and search
- **API Response Times:** Monitor B2B pricing API performance
- **Mobile Performance:** Test filter responsiveness on mobile devices
- **Cumulative Layout Shift:** Searchspring gets loaded late on the page. Monitor for any layout shifts that could impact user experience and core web vitals.

### Code Maintenance

> **Important:** Any changes to the SearchSpring codebase should be tested on all three sites (OneSource, FastServ, Winzer Corp) to ensure site-specific features continue to function correctly.

---

## Questions or Issues?

For technical questions about this SearchSpring implementation, contact:

**Arcadia Digital**  
[pete.buzzell@arcadiadigital.com](mailto:pete.buzzell@arcadiadigital.com)

## Additional Resources

For general SearchSpring questions and documentation:

- **SearchSpring User Documentation:** [https://help.searchspring.net/hc/en-us](https://help.searchspring.net/hc/en-us)
- **SearchSpring API Reference:** [https://docs.searchspring.com/reference/getting-started-welcome](https://docs.searchspring.com/reference/getting-started-welcome)

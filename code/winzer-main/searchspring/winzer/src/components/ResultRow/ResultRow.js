/** @jsx jsx */
import { Fragment, h } from 'preact'
import { useEffect, useState, useMemo } from 'preact/hooks'
import { useRef } from 'react';

// Searchspring
import { Price, Results, useMediaQuery } from '@searchspring/snap-preact-components'
import { useTheme, CacheProvider } from '@searchspring/snap-preact-components/dist/cjs/providers'
import { defined, cloneWithProps } from '@searchspring/snap-preact-components/dist/cjs/utilities'
import { withController } from '@searchspring/snap-preact-components';


// Packages
import { jsx, css } from '@emotion/react'
import classnames from 'classnames'
import { observer } from 'mobx-react-lite'

// Local
import { useSwatches } from '../Helpers'
import { MediaQuery } from '../Helpers/MediaQuery';
import { VariantRow } from '../ResultVariantRow/ResultVariantRow';

// Icons
import { ChevronDown } from '../Icons/ChevronDown'
import { SortArrow } from '../Icons/SortArrow'
import { OpenVariantIcon } from '../Icons/OpenVariantIcon'
import { NoImage } from '../Icons/NoImage'
import { LoadingIcon } from '../Icons/LoadingIcon'
import { InfoIcon } from '../Icons/InfoIcon'

// CSS
import './resultrow.css'

const CSS = {
    result: () =>
        css({
            '&.ss__result--grid': {
                display: 'flex',
                flexDirection: 'column',
                height: '100%',
                '& .ss__row__image-wrapper': {
                    flex: '1 0 auto',
                },
            },
            '&.ss__result--list': {
                display: 'flex',
                flexDirection: 'row',
                '& .ss__row__image-wrapper': {
                    flex: '0 0 33%',
                },
                '& .ss__row__details': {
                    flex: '1 1 auto',
                    textAlign: 'left',
                    marginLeft: '20px',
                    padding: 0,
                },
            },

            '& .ss__row__image-wrapper': {
                position: 'relative',
                '& .ss__result__badge': {
                    background: '#FFFFFF',
                },
            },

            '& .ss__row__details': {

                '& .ss__row__details__title': {
                    marginBottom: '10px',
                },
                '& .ss__row__details__pricing': {
                    marginBottom: '10px',

                    '& .ss__result__price': {
                        fontSize: '1.2em',
                    },
                    '& .ss__price--strike': {
                        fontSize: '80%',
                    },
                },
                '& .ss__row__details__button': {
                    marginBottom: '10px',
                },
            },
        }),
}

// general "helper" variables
const plpLocales = window?.Resources?.searchspring?.results;
const sitename = window?.Resources?.searchspring?.liquid_data?.sitename;
const signed_in = window?.Resources?.searchspring?.liquid_data?.signed_in;

const siteFeatures = {
  winzeronesource: {
    bulk_pricing: true,
		force_sign_in: false,
    exact_pricing: false,
    b2b_pricing: false
  },
  winzerfastserv: {
    bulk_pricing: false,
		force_sign_in: true,
    exact_pricing: true,
    b2b_pricing: true
  },
  winzercorp: {
    bulk_pricing: false,
		force_sign_in: true,
    exact_pricing: true,
    b2b_pricing: true
  }
}

export const currentSiteFeatures = siteFeatures[sitename];
export const force_sign_in = (currentSiteFeatures.force_sign_in && !signed_in);

/*=========================================
=====      The Result/Product Row     =====
=========================================*/

export const ResultRow = observer(properties => {
    const globalTheme = useTheme()

    // initialize state variables
    const [viewAllVariants, setViewAllVariants] = useState(false);
    const [sortParams, setSortParams] = useState({
      option: null,
      asc: false
    });
    const [isExactPricingLoading, setIsExactPricingLoading] = useState(false);
    const [variantExactPricing, setVariantExactPricing] = useState(null);
    const [scrollPosition, setScrollPosition] = useState(0);
    const [b2bPricing, setb2bPricing] = useState(null);

    // establish prop variables
    const props = {
        layout: 'grid',
        ...globalTheme?.components?.result,
        ...properties,
        ...properties.theme?.components?.result,
    }

    const {
        result,
        hideBadge,
        hideTitle,
        detailSlot,
        fallback,
        disableStyles,
        className,
        layout,
        style,
        controller,
        filters,
    } = props

    const core = result?.mappings?.core;
		// add collection url to product links
    const coreUrl = core.url;
    const productID = core.uid;
    const hoverImage = result?.attributes?.ss_image_hover;
		// default badge metafield removed?:
    // WHEN WE HAVE DATA: check which badge metafield is coming through
    // const badge = result?.attributes?.mfield_cql_badge_label?.length ? result?.attributes?.mfield_cql_badge_label : null; 
		const badge = result?.attributes?.mfield_cql_product_badges?.length ? JSON.parse(result?.attributes?.mfield_cql_product_badges)[0] : null;
    const priceRange = result?.attributes?.ss_price_range ? result?.attributes?.ss_price_range : null
    const vendor = result?.attributes?.vendor;
    const options = result?.attributes?.options ? JSON.parse(result?.attributes?.options) : result?.attributes?.options;
    const variants = result?.attributes?.variants ? JSON.parse(result?.attributes?.variants) : result?.attributes?.variants;
    const isDefaultTitle = variants?.[0]?.title === "Default Title" && variants?.length === 1 && options?.length === 1;

    const styling = {}
    if (!disableStyles) {
        styling.css = [CSS.result(), style]
    } else if (style) {
        styling.css = [style]
    }

    /********************
      Handler functions:
        handle sorting
        handle view all variants
        handle loading exact pricing
    ********************/

    function handleSortParams(e) {
      const sortedOption = e.currentTarget.dataset.option;
      const asc = (sortedOption === sortParams.option) ? !sortParams.asc : true;

      setSortParams({
        option: sortedOption,
        asc: asc
      });
    }

    function handleViewAll(e) {
      setViewAllVariants(!viewAllVariants);
      if (viewAllVariants) {
        window.scrollTo({
          top: scrollPosition,
          behavior: 'auto'
        });
      } else {
        const currentScrollPosition = window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop;
        setScrollPosition(currentScrollPosition);
      }
    }

    function loadExactPricing(){
      const companyId = window.Resources.searchspring.exact_pricing.user_company_id;
      const productId = core.uid;

      setIsExactPricingLoading(true);

      fetch(`/apps/pricing-api/customer-product-pricing?current_company_id=${companyId}&product_ids=${productId}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
      })
      .then(response => response.json())
      .then(data => {
          // assumes only one product id is passed through
          const variantExactPrices = data[0].variants;

          if(!!variantExactPrices) setVariantExactPricing(variantExactPrices);
      })
      .catch(error => {
          console.error('Error fetching pricing:', error);
      }).finally(() => {
        setIsExactPricingLoading(false);
      });
    }

    /********************
      Helper function:
        detect if a product has ANY variant that is purchasable/in catalog
        fetch b2b pricing from PDP as SS can't access that info
    ********************/

    function anyVariantInCatalog(pricing) {
      if (pricing) {
        return Object.values(pricing).reduce((anyInCatalog, p) => {
          let numberedPrice = parseFloat(p.replaceAll(',', '').replaceAll('$', ''));
          return anyInCatalog || (numberedPrice < 1000000 && numberedPrice > 0);
        }, false);
      } else {
        return !currentSiteFeatures.b2b_pricing;
      }
    }

    const fetchB2BPricing = async () => {
      // api call to the PDP using the pricing_json template, returning json: {<variant_id>: <variant_price>}
      const pricing_json_response = await fetch(`${coreUrl}?view=pricing_json`);
      const pricing_json = await pricing_json_response.json();
      setb2bPricing(pricing_json);
    }
    // sites using b2b_pricing need this, so fetch immediatly if it hasn't been already
    if (!b2bPricing && currentSiteFeatures.b2b_pricing) fetchB2BPricing();

    return (
        <CacheProvider>
            <article
              {...styling}
              className={classnames('ss__result_row', (typeof hoverImage != "undefined" && 'ss__result__hover'), (isDefaultTitle && 'default-title'), `ss__result--${layout}`, className)}
            >
              <div className="product-row-top-wrapper">
                <div class="product-data-wrapper">
                  <div className="ss__row__image-wrapper">
                    <a
                      href={coreUrl} // href={core.url + queryParam}
                      onMouseDown={e => {
                          controller?.track?.product?.click(e, result)
                      }}
                      aria-label={core.name}
                    >
                      {core.thumbnailImageUrl &&
                        <img
                          id={`ProductImage-${result.id}`}
                          alt={core.name}
                          className="lazyload"
                          data-aspectratio="1"
                          data-sizes="auto"
                          data-image
                          src={core.thumbnailImageUrl}
                        />
                      }
                      {!core.thumbnailImageUrl &&
                        <NoImage />
                      }
                    </a>
                  </div>
                  <div className="ss__row__details">
                    <div className="eyebrow">
                      <span className="vendor">{(vendor ? vendor : window.Resources.shop_name)}</span>
                      {(badge && !hideBadge) && <span className="row-badge">{badge}</span>}
                    </div>
                    {!hideTitle && (
                      <a
                        href={coreUrl} // href={core.url + queryParam}
                        onMouseDown={e => {
                            controller?.track?.product?.click(e, result)
                        }}
                        aria-label={core.name}
                      >
                        <h6
                          className="h6"
                          dangerouslySetInnerHTML={{
                            __html: core.name,
                          }}
                        />
                      </a>
                    )}

                    <MediaQuery query="(max-width: 1199px)">
                      {/* "close all varints" button for mobile for all products */}
                      {variants && <ViewAllVariantsButton
                        viewAllVariants={viewAllVariants}
                        handleViewAll={handleViewAll}
                        count={variants.length}
                        productTitle=""
                      />}
                    </MediaQuery>
                    <MediaQuery query="(min-width: 1200px)">
                      {/* "close all varints" button for desktop to account for the display conditions */}
                      {(variants && viewAllVariants && variants.length > 10) &&
                      <ViewAllVariantsButton
                        viewAllVariants={viewAllVariants}
                        handleViewAll={handleViewAll}
                        count={variants.length}
                        productTitle=""
                      />}
                    </MediaQuery>
                    {cloneWithProps(detailSlot, { result })}
                  </div>
                </div>
								{
                  options && <>
                    <VariantTableHeader
                      isDefaultTitle={isDefaultTitle}
                      options={options}
                      sortParams={sortParams}
                      handleSortParams={handleSortParams}
                      variantExactPricing={variantExactPricing}
                      loadExactPricing={loadExactPricing}
                      isExactPricingLoading={isExactPricingLoading}
                    />
                    {(currentSiteFeatures.exact_pricing && signed_in && !variantExactPricing && anyVariantInCatalog(b2bPricing)) && <ViewExactPricingCta
                      loadExactPricing={loadExactPricing}
                      isExactPricingLoading={isExactPricingLoading}
                    />}
                  </>
                }
              </div>
              {
                (options && variants) &&
                <VariantTable
                  // Primary Data modals the component is built off of
                  options={options}
                  variants={variants}
                  sortParams={sortParams} // current sort parameters for the table
                  filters={filters} // current filters parameters for products, passing down to filter the variants as well
                  // Handler Functions
                  handleViewAll={handleViewAll}
                  loadExactPricing={loadExactPricing}
                  // product-level data the variant componant needs
                  isDefaultTitle={isDefaultTitle}
                  productId={core.uid}
                  productTitle={core.name}
                  productUrl={coreUrl}
                  productImageUrl={core.thumbnailImageUrl}
                  attributes={result?.attributes}
                  // ResultRow state variables
                  variantExactPricing={variantExactPricing}
                  viewAllVariants={viewAllVariants}
                  isExactPricingLoading={isExactPricingLoading}
                  b2bPricing={b2bPricing}
                  // ResultRow state functions 
                  fetchB2BPricing={fetchB2BPricing}
                />
              }
            </article>
        </CacheProvider>
    )
})

/*=========================================
=====      Header for the Table       =====
=========================================*/

const VariantTableHeader = (props) => {
  const enablePriceSort = !props.isDefaultTitle && !force_sign_in;

  return (
    <div className="variant-rows-header">
			<div className="variant-rows-header-item">{plpLocales?.sku}</div>
			{
				!props.isDefaultTitle && props.options.map((option) => {
					return (
						<div 
            className={classnames("variant-rows-header-item", (props.sortParams.option === `option${option.position}` && "currently-sorting"), ((props.sortParams.asc && props.sortParams.option === `option${option.position}`) && "asc"))}
              data-option={`option${option.position}`}
              onClick={props.handleSortParams}
            >
              <span>{option.name}</span>
              <SortArrow />
            </div>
					)
				})
			}
			<div
        className={classnames("variant-rows-header-item", (props.sortParams.option === `price` && "currently-sorting"), ((props.sortParams.asc && props.sortParams.option === `price`) && "asc"))}
        data-option={enablePriceSort && "price"}
        onClick={enablePriceSort && props.handleSortParams}
      >
        {plpLocales?.price}
        {enablePriceSort && <SortArrow />}
      </div>
    </div>
  );
}

/*=========================================
=====      The Table of Variants      =====
=========================================*/

const VariantTable = (props) => {

  /********************
    Helper functions:
      filter function to apply SS's currently active filters to the variant's within products
      sort function to execute <ResultRow>'s handleSorting
        distinguishes pure numbers but otherwise sorts alphabetically
  ********************/

  function filterVariants(variants, options, filters) {

    // format the ss key into the string format used in the attributes metafield ("ss_generic_color" => "Generic Color")
    function extractFromAttributes(key, attributesField) {
      if (!attributesField) {
        return false;
      }
      
      const attributes_json = JSON.parse(attributesField);
      const formattedKey = key.replace('ss_', '').split('_').map(g => g.charAt(0).toUpperCase() + g.slice(1)).join(' ');

      return attributes_json[formattedKey] ?? false;
    }

    // reduce set of current filters into an object with key/values of "field_name": ["Array", "of", "values"]
    const filter_map = filters.reduce((f_map, f) => {
      let value = f.value.value;
      let key = f.facet.field.replace('variant_', '');
      // if the filter is based on an option, set the key to option1/option2/option3 based on product's options
      const matchingPosition = options.filter((o) => o.name.toLocaleLowerCase().replace(/[^a-z0-9]+/g, '_') === key)[0]?.position;
      key = matchingPosition ? `option${matchingPosition}` : key;

      if (key === "ss_price") {
        f_map[key] = f.value; // price filter needs to record the high & low instead of an array of values
      } else {
        f_map[key] = f_map[key] ? [...f_map[key], value] : [value];
      }
  
      return f_map;
    }, {});

    return variants.filter((v) => {
      // for each "facet" or key in the map, return true if ANY value within the fact matches
      // return true for the whole variant if every facet evaluates to true
      return Object.keys(filter_map).map((key) => {
        if (key === "ss_price") {
          return filter_map[key].low <= parseFloat(v.price) && parseFloat(v.price) <= filter_map[key].high;
        } else if (v[key]) {
          // split on "|" to catch multi-value fields
          return v[key].split('|').some((v) => filter_map[key].indexOf(v) > -1);
        } else if (extractFromAttributes(key, v.mfield_cql_attributes_json)) {
          // check if field is one of the attributes from mfield_cql_attributes_json and extract it from there
          // also split on "|" to catch multi-value fields
          return extractFromAttributes(key, v.mfield_cql_attributes_json).split('|').some((v) => filter_map[key].indexOf(v) > -1);
        } else {
          // if no matching variant field applies, it's probably a product field that's already accounted for
          // UNLESS!! the key contains ss_generic, in which case it is probably looking, but not finding, a field from inside mfield_cql_attributes_json
          return key.indexOf('ss_generic') === -1;
        }
      }).every((t) => t);
    });
  }

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

  // filter variants based on current filters
  // then sort variants based on current sorting params
  // finally trim based on if "view all" is selected
  const filtedVariants = filterVariants(props.variants, props.options, props.filters);
  const sortedVariants = props.sortParams.option ? sortVariants(filtedVariants, props.sortParams) : filtedVariants;
  const trimmedVariants = props.viewAllVariants && sortedVariants.length > 10 ? sortedVariants : sortedVariants.slice(0, 10);

  return (
    <div className="product-row-variant-table">
      <div className={classnames("variant-rows-container", props.viewAllVariants && "view-all-mobile")}>
				{trimmedVariants.map((variant) => <Fragment key={variant.id}>
            <VariantRow
              // Primary Data modals the component is built off of
              options={props.options}
              variant={variant}
              // extra variant-level data
              b2bPrice={props.b2bPricing?.[variant.id]}
              fetchB2BPricing={props.fetchB2BPricing}
              // product-level data the variant componant needs
              isDefaultTitle={props.isDefaultTitle}
              productTitle={props.productTitle}
              productId={props.productId}
              productUrl={props.productUrl}
              productImageUrl={props.productImageUrl}
              attributes={props.attributes}
              // Handler Function
              loadExactPricing={props.loadExactPricing}
              // ResultRow state variables
              isExactPricingLoading={props.isExactPricingLoading}
              variantExactPricing={props.variantExactPricing} // this is product level because it contains all variant's exact pricing
            />
          </Fragment>
        )}
        {/* conditional "view all"/"close All" button */}
        {
          (sortedVariants.length > 10) &&
          <ViewAllVariantsButton
            viewAllVariants={props.viewAllVariants}
            handleViewAll={props.handleViewAll}
            count={sortedVariants.length}
            productTitle={props.productTitle}
          />
        }
      </div>
    </div>
  );
}

/*==============================================================
===== Button To Show/Hide Variant Rows -- Desktop & Mobile =====
==============================================================*/

const ViewAllVariantsButton = (props) => {
  return (
    <div className={classnames("view-all-variants", (props.viewAllVariants && "open"))} onClick={props.handleViewAll}>
      <span className="link">{props.viewAllVariants ? "Close All" : "View All"} Options ({props.count})</span> {props.productTitle} <OpenVariantIcon />
    </div>
  );
}

/*==============================================================
=====    CTA to fetch 4 digit pricing where applicable     =====
==============================================================*/

export const ViewExactPricingCta = ({
  loadExactPricing,
  isExactPricingLoading }) => {

  return (
    <div class="exact-pricing-container">
      <button
        class="plp-exact-pricing-cta button-reset"
        onClick={loadExactPricing}
        >
          {plpLocales.cta_text}

          <div class="tooltip-container" tabindex="0" aria-label="icon show .0000 pricing tooltip">
            <span aria-live="polite" aria-busy={isExactPricingLoading}>
              {isExactPricingLoading ? (
                <span class="loading-animation" aria-label="Loading exact pricing…">
                  <LoadingIcon />
                </span>
              ) : (
                <>
                  <span class="tooltip-icon">
                    <InfoIcon />
                  </span>
                  <div class="tooltip-box" role="tooltip" dangerouslySetInnerHTML={{ __html: plpLocales.tooltip_msg}} />
                </>
              )}
            </span>
          </div>
        </button>
    </div>
  );
}



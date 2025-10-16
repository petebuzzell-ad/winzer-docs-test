/** @jsx jsx */
import { Fragment, h } from 'preact'
import { useState, useMemo } from 'preact/hooks'
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
import { IconMore } from '../Icons/icon_more'
import { StarEmpty } from '../Icons/StarEmpty';
import { StarFull } from '../Icons/StarFull';
import { StarHalf } from '../Icons/StarHalf';

// CSS
import './result.css'

const signed_in = window?.Resources?.searchspring?.liquid_data?.signed_in;
const plpLocales = window?.Resources?.searchspring?.results;
const sitename = window?.Resources?.searchspring?.liquid_data?.sitename;

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

const CSS = {
    result: () =>
        css({
            '&.ss__result--grid': {
                display: 'flex',
                flexDirection: 'column',
                height: '100%',
                '& .ss__result__image-wrapper': {
                    flex: '1 0 auto',
                },
            },
            '&.ss__result--list': {
                display: 'flex',
                flexDirection: 'row',
                '& .ss__result__image-wrapper': {
                    flex: '0 0 33%',
                },
                '& .ss__result__details': {
                    flex: '1 1 auto',
                    textAlign: 'left',
                    marginLeft: '20px',
                    padding: 0,
                },
            },
        }),
}

function mode(arr){
    return arr.sort((a,b) =>
          arr.filter(v => v===a).length
        - arr.filter(v => v===b).length
    ).pop();
}

export const Result = observer(properties => {
    const globalTheme = useTheme()
    const [swatchIndex, setSwatchIndex] = useState(0)

    const props = {
        layout: 'grid',
        ...globalTheme?.components?.result,
        ...properties,
        ...properties.theme?.components?.result,
    }

    const {
        result,
        hideBadge,
        hideSwatches,
        hidePricing,
        hideTitle,
        hideWishlist,
        hideReviews,
        detailSlot,
        fallback,
        disableStyles,
        className,
        layout,
        style,
        controller,
        query
    } = props

    const core = result?.mappings?.core
    const productID = core.uid;
    const hoverImage = result?.attributes?.ss_image_hover;
    const badge = result?.attributes?.mfield_cql_badge_label?.length ? result?.attributes?.mfield_cql_badge_label : null
    const priceRange = result?.attributes?.ss_price_range ? result?.attributes?.ss_price_range : null
    const variantColors = result?.attributes?.variant_color
    const variantImages = result?.attributes?.variant_images_json
    const variantID = result?.attributes?.variant_id ? result?.attributes?.variant_id : productID;
    let variantIDArray = "";
    const swatchImages = result?.attributes.mfield_cql_swatches_json
    const reviewScore = result?.attributes.reviews_product_score;
    const reviewTotal = result?.attributes.reviews_total_reviews;
    let shouldSkip = false;
    const vendorName = result?.attributes?.mfield_cql_vendor_name ?? window.Resources.shop_name;
    const productBadges = !!result?.attributes?.mfield_cql_product_badges ? JSON.parse(result?.attributes?.mfield_cql_product_badges) : [];
    const promoMessaging = result?.attributes?.mfield_cql_promo_messaging;
    const variants = result?.attributes?.variants ? JSON.parse(result?.attributes?.variants): null;
    const package_display = variants?.map((v) => v.mfield_cql_package_display);
    const unitOfMeasure = package_display ? mode(package_display) : null;

    const swatches = useSwatches({
        variantColors: variantColors,
        variantImages: variantImages,
        swatchImages: swatchImages,
        fallbackImage: core.imageUrl,
    })


    const subProps = {
        price: {
            className: 'ss__result__price',
            ...globalTheme?.components?.price,
            ...defined({
                disableStyles,
            }),
            theme: props.theme,
        },
        badge: {
            className: 'ss__result__badge',
            content: badge,
            ...globalTheme?.components?.badge,
            ...defined({
                disableStyles,
            }),
            theme: props.theme,
        },
        image: {
            className: 'ss__result__image',
            alt: core?.name,
            src: core?.imageUrl,
            ...globalTheme?.components?.image,
            ...defined({
                disableStyles,
                fallback: fallback,
            }),
            theme: props.theme,
        },
    }

    const styling = {}
    if (!disableStyles) {
        styling.css = [CSS.result(), style]
    } else if (style) {
        styling.css = [style]
    }

    const queryParam = variants.filter(v => v.sku.toLowerCase().indexOf(query?.toLowerCase()) > -1).map(v => v.id)[0];

    const displayPrices = (priceRange) => {

        let decimalPlaces = 0;
        let corePrice = String(core.price);
        let msrpPrice = String(core.msrp);

        if (currentSiteFeatures.b2b_pricing) {
            if (b2bPricing) {
                let b2bNumbers = Object.values(b2bPricing).map((p) => parseFloat(p.replaceAll(',', '').replaceAll('$', '')));
                if (Math.max(...b2bNumbers) === 0) {
                    corePrice = String(0);
                } else {
                    corePrice = String(Math.min(...b2bNumbers.filter((n) => n > 0)));
                }
                if (parseInt(corePrice) >= 1000000 || parseInt(corePrice) <= 0) {
                    return <span className="not-in-catalog">{plpLocales.not_in_catalog}</span>;
                }
            } else {
                return <span class="loading-price"></span>;
            }
        }

        if (corePrice.includes('.') || msrpPrice.includes('.')) {
            decimalPlaces = 2;
        }

        if (priceRange) {
            let priceRange1 = String(priceRange[1]);
            let priceRange2 = String(priceRange[0]);

            if (priceRange1.includes('.') || priceRange2.includes('.')) {
                decimalPlaces = 2;
            }

            return (
                <>
                    <Price {...subProps.price} value={Number(priceRange[1])} decimalPlaces={decimalPlaces} />
                    <span class="em-dash">&#8211;</span>
                    <Price {...subProps.price} value={Number(priceRange[0])} decimalPlaces={decimalPlaces} />
                </>
            )
        }

        //otherwise, return core price
        return (
            <>
               {core.price < core.msrp ? (
                    <>
                        <Price {...subProps.price} value={parseFloat(corePrice)} decimalPlaces={decimalPlaces} />&nbsp;<Price {...subProps.price} value={core.msrp} lineThrough={true} decimalPlaces={decimalPlaces} />
                    </>
                ) : (
                    <Price {...subProps.price} value={parseFloat(corePrice)} decimalPlaces={decimalPlaces} />
                )}
            </>
        )
    }

    const [selectedSwatch, setSelectedSwatch] = useState(0);
    const [selectedURL, setSelectedURL] = useState(variantIDArray[0]);

    const [imageChange, setImageChange] = useState();
    const [b2bPricing, setb2bPricing] = useState(null);

    const fetchB2BPricing = async () => {
    // api call to the PDP using the pricing_json template, returning json: {<variant_id>: <variant_price>}
    const pricing_json_response = await fetch(`${core.url}?view=pricing_json`);
    const pricing_json = await pricing_json_response.json();
    setb2bPricing(pricing_json);
    }
    // sites using b2b_pricing need this, so fetch immediatly if it hasn't been already
    if (!b2bPricing && currentSiteFeatures.b2b_pricing) fetchB2BPricing();



    const DynamicVariants = withController(
        observer(({ controller, result }) => {

            const urlParams = new URLSearchParams(window.location.search);

            if(!urlParams.get('q') && imageChange != true) {
                core.imageUrl = JSON.parse(variantImages)[0].img;
                setImageChange(true);
            }

            const store = controller.store;
            const variantsConfig = store.custom.variantsConfig;
            const { attributes } = result;
            const intellisuggest = (e) => controller.track.product.click(e, result);


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

            const swatchList = [...swatches];

            return variantsConfig.swap && attributes[variantsConfig.field] && attributes[variantsConfig.field].length !== 0 ? (
                <>
                    <div className="all-finishes">

                        {attributes[variantsConfig.field].slice(0, variantsConfig.limit).map((swatch, i) =>
                            i <= 3 ? (
                                <div id={swatch.id} className="finish-parent">
                                    <div
                                        className={selectedSwatch == i ? "finish selectedSwatch" : "finish" }
                                        onClick={(e) => {
                                            variantsConfig.swap(result, swatch);
                                            intellisuggest(e);
                                            handleSelectedClass(e, i);
                                            handleSelectedURL();
                                        }}
                                        title={swatch?.colourfamily}
                                    >
                                            {swatchList.filter(swatchImg => swatch.color === swatchImg.color)
                                            .map((swatchImg) => (
                                                <img
                                                    src={swatchImg.swatch_image}
                                                    alt={swatch.color}
                                                    width={25}
                                                    height={25}
                                                    loading="lazy"
                                                    swatchIndex={i}
                                                />
                                            ))}

                                    </div>
                                </div>
                            ) : null
                        )}

                        {attributes[variantsConfig.field].length > 4 ? (
                            <span className="more-finishes-ava">+{attributes[variantsConfig.field].length - 4}</span>
                        ) : null}
                    </div>
                </>
            ) : null;
        }));



    return (
        <CacheProvider>
            <article
                {...styling}
                className={classnames('ss__result', (typeof hoverImage != "undefined" && 'ss__result__hover'), `ss__result--${layout}`, className)}
            >
                <a
                    href={(queryParam ? `${core.url}?variant=${queryParam}` : core.url)}
                    onMouseDown={e => {
                        controller?.track?.product?.click(e, result)
                    }}
                    className="ss__result-link link"
                    aria-label={core.name}
                >
                    <div className="product-card-top-wrapper">
                        <div className="ss__result__image-wrapper">
                            {!hideBadge && !!productBadges[0] && (
                                <span className="badge ss__badge ss__result__badge">
                                    {productBadges[0]}
                                </span>
                            )}
                            {!hideWishlist && (
                                <div ref={ref} id="" class="swym-button--wrap" data-variant-id={queryParamVariantID}>
                                    <a class={`swym-button swym-add-to-wishlist-view-product product_${result.id}`}
                                        data-with-epi="true"
                                        data-swaction="addToWishlist"
                                        data-product-id={result.id}
                                        data-product-url={core.url + queryParamVariantIdUrl}
                                        data-variant-id={queryParamVariantID}
                                        onClick={onItemClick}
                                        >
                                    </a>
                                </div>
                            )}
                            <div
                                {...styling}
                                className={classnames('ss__image', 'ss__result__image')}
                            >
                                {!!core.imageUrl ?
                                     <>
                                     <img
                                        id={`ProductImage-${result.id}`}
                                        alt={core.name}
                                        class="product__img product__img--main lazyload"
                                        data-aspectratio="1.4967637540453074"
                                        data-sizes="auto"
                                        data-image
                                        src={core.imageUrl}
                                        />

                                        {typeof hoverImage != "undefined" &&
                                            <img
                                            id={`ProductImage-${result.id}`}
                                            alt={core.name}
                                            class="product__img product__img--hover lazyload"
                                            data-aspectratio="1.4967637540453074"
                                            data-sizes="auto"
                                            data-image
                                            src={hoverImage}
                                        />
                                        }
                                     </>
                                    :
                                    <span class="ss__result__no-image">
                                        <svg class="icon icon-no-image" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 590 590">
                                            <g fill="#C4C3C2" clip-path="url(#a)">
                                                <path d="M412.909 178.091a7.127 7.127 0 0 0-10.074 0L178.091 402.835a7.127 7.127 0 0 0 0 10.074 7.067 7.067 0 0 0 5.037 2.091c1.829 0 3.659-.689 5.037-2.091l37.849-37.849h163.207c8.173 0 14.802-6.629 14.802-14.803V230.743c0-8.174-6.629-14.803-14.802-14.803h-4.087l27.775-27.775a7.127 7.127 0 0 0 0-10.074Zm-58.615 117.421c0 32.408-26.374 58.782-58.782 58.782-13.638 0-26.207-4.681-36.186-12.522l10.193-10.193c7.318 5.299 16.275 8.459 25.993 8.459 24.544 0 44.526-19.982 44.526-44.526 0-9.718-3.16-18.675-8.459-25.993l10.193-10.193c7.841 9.979 12.522 22.548 12.522 36.186ZM241.909 319.604a58.416 58.416 0 0 1-5.203-24.116c0-32.408 26.373-58.782 58.782-58.782 8.577 0 16.75 1.877 24.116 5.204l34.855-34.856c-2.922-4.514-8.007-7.508-13.78-7.508H250.32c-9.076 0-16.418 7.342-16.418 16.418h-32.123c-8.173 0-14.802 6.629-14.802 14.802v129.515c0 3.968 1.568 7.532 4.086 10.193l50.846-50.846v-.024Z"></path>
                                                <path d="M250.962 295.512c0 4.538.689 8.934 1.972 13.068l55.645-55.646a44.047 44.047 0 0 0-13.067-1.972c-24.544 0-44.526 19.982-44.526 44.526l-.024.024Z"></path>
                                            </g>
                                            <defs>
                                                <clipPath id="a">
                                                <path fill="#fff" d="M0 0h239v239H0z" transform="translate(176 176)"></path>
                                                </clipPath>
                                            </defs>
                                            </svg>
                                    </span>
                                }

                            </div>
                        </div>
                        <div className="ss__result__details">
                            {!hideReviews && (
                                <div className="ss__result__details__review">
                                    {/* Leaving this comment below for now */}
                                    {/* <div class="yotpo bottomLine" data-product-id={ core.uid }></div> */}

                                    {reviewScore == 5 &&
                                        <>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarFull/>
                                        </>
                                    }

                                    {reviewScore > 4 && reviewScore < 5 &&
                                        <>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarHalf/>
                                        </>
                                    }

                                    {reviewScore == 4 &&
                                        <>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarEmpty/>
                                        </>
                                    }

                                    {reviewScore > 3 && reviewScore < 4 &&
                                        <>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarHalf/>
                                            <StarEmpty/>
                                        </>
                                    }

                                    {reviewScore == 3 &&
                                        <>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                        </>
                                    }

                                    {reviewScore > 2 && reviewScore < 3 &&
                                        <>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarHalf/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                        </>
                                    }

                                    {reviewScore == 2 &&
                                        <>
                                            <StarFull/>
                                            <StarFull/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                        </>
                                    }

                                    {reviewScore > 1 && reviewScore < 2 &&
                                        <>
                                            <StarFull/>
                                            <StarHalf/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                        </>
                                    }

                                    {reviewScore == 1 &&
                                        <>
                                            <StarFull/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                        </>
                                    }

                                    {reviewScore > 0 && reviewScore < 1 &&
                                        <>
                                            <StarHalf/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                        </>
                                    }

                                    {reviewScore == 0 &&
                                        <>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                            <StarEmpty/>
                                        </>
                                    }

                                    {reviewScore == undefined &&
                                        <div className="ss__results__no-reviews">{/* {window.Resources.searchspring.results.noReviews} */}</div>
                                    }
                                </div>
                            )}
                            { !!vendorName &&
                                <span class="eyebrow ss__result__vendor">{vendorName}</span>
                            }
                            {!hideTitle && (
                                <div
                                    className="ss__result__details__title"
                                    dangerouslySetInnerHTML={{
                                        __html: core.name,
                                    }}
                                />
                            )}
                            {!hidePricing && (
                                force_sign_in ? (                                    
                                    <a target="_blank" href={window.routes.account_login_url}>{plpLocales.compact_sign_in_link} {plpLocales.compact_sign_in_text}</a>
                                ) : (<div>
                                    <span className="ss__result__details__pricing">
                                    {displayPrices(priceRange)}
                                    </span>
                                    {(!!unitOfMeasure && displayPrices(priceRange).props.className !== "not-in-catalog") &&
                                    <span class="ss__result__uom">{` ${unitOfMeasure}`}</span>
                                    }
                                </div>)
                            )}
                            {!!promoMessaging &&
                                <span className="ss__results__promo-msg">{promoMessaging}</span>
                            }
                            {cloneWithProps(detailSlot, { result })}
                        </div>
                    </div>
                    </a>
                    <div className="product-card-bottom-wrapper">
                        {!hideSwatches && swatches.size > 1 ? (
                            <div>
                            <DynamicVariants
                                result={result}
                                // Do not pass {controller} here
                            />
                            </div>
                        ) : null}
                    </div>
            </article>
        </CacheProvider>
    )
})

const ColorSwatches = ({ swatches, setSwatchIndex, swatchIndex }) => {
    const breakNumber = 4
    const swatchList = [...swatches];

    return (
        <div className="ss__result__swatches">
            {swatchList.map((swatch, index) => {
                if (index > breakNumber) return null
                return (
                    <>
                        {index === breakNumber ? (
                            <IconMore
                                count = {swatchList.length}
                                breakpoint = {breakNumber}
                            />
                        ) : (
                            <div
                                className={classnames('swatch-image', {
                                    'swatch-image-selected': swatchIndex === index,
                                })}
                                title={swatch.color}
                                data-index={index}
                                data-thumbnail={swatch.thumbnail}
                            >

                            {swatch.swatch_image ? (
                                <img
                                    src={swatch.swatch_image}
                                    onClick={e => {
                                        e.preventDefault()
                                        setSwatchIndex(index)
                                    }}
                                    alt={swatch.color}
                                    width={25}
                                    height={25}
                                    loading="lazy"
                                />
                            ) : null}

                            </div>
                        )}
                    </>
                )
            })}
        </div>
    )
}

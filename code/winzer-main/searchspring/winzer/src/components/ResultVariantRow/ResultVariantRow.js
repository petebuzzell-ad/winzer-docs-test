import { Fragment, h } from 'preact'
import { useState } from 'preact/hooks'
import classnames from 'classnames'

// also import some helper variables
// currentSiteFeatures =  { bulk_pricing, force_sign_in, exact_pricing, b2b_pricing }
import { currentSiteFeatures, force_sign_in, ViewExactPricingCta } from '../ResultRow/ResultRow'

import { OpenVariantIcon } from '../Icons/OpenVariantIcon'
import { NoImage } from '../Icons/NoImage'
import { PlusIcon } from '../Icons/PlusIcon'
import { MinusIcon } from '../Icons/MinusIcon'
import { ATCIcon } from '../Icons/ATCIcon'
import { LinkArrowIcon } from '../Icons/LinkArrowIcon'
import { ShippingIcon } from '../Icons/ShippingIcon'
import { BoxIcon } from '../Icons/BoxIcon'
import { LoadingIcon } from '../Icons/LoadingIcon'

// CSS
import './resultvariantrow.css'

// global helper variables
const plpLocales = window?.Resources?.searchspring?.results;

// helper to sort metafields with reduce
function sortMetafieldArray(obj, item) {
	return {...obj, [`${item.namespace}_${item.key}`]: item.value};
}

/*=========================================
=====         The Variant Row         =====
=========================================*/

export const VariantRow = (props) => {
	// establish prop variables
	const variant = props.variant;
	const variantMetafields = variant.metafields.reduce(sortMetafieldArray, {});
	const variantUrl = `${props.productUrl}?variant=${variant.id}`;
	const {
		cql_package_display,
		cql_minimum_order_quantity
	} = variantMetafields;

	const unitOfMeasure = cql_package_display ? cql_package_display : null;
	
	// initialize state variables
	const [quantity, setQuantity] = useState(parseInt(cql_minimum_order_quantity) || 1); // need ATC quantity set here for determining qty breaks price
	const [detailsOpen, setDetailsOpen] = useState(false);
	const [bulkPricingData, setBulkPricingData] = useState(false);
	const [foundBulkPricingData, setFoundBulkPricingData] = useState("searching"); // possible states: "searching", "loaded", "unavaible"
	const [displayBulkPricingData, setDisplayBulkPricingData] = useState("hide"); // possible states: "hide", "show", "load"

	/********************
		Helper functions:
			not In Your Catalog
				if the price is 1000000 or 0 don't allow purchase
			fetch Bulk Pricing Data
				copied & modifed from /sites/onesource/assets/product-form.js
			fetch Exact Pricing Data
				grab a specific variant's price from product's fetched ExactPricing
	********************/

	function notInYourCatalog(price) {
		if (!currentSiteFeatures.b2b_pricing) {
			return false;
		} else if (props.b2bPrice) {
			let numberedPrice = parseFloat(props.b2bPrice.replaceAll(',', '').replaceAll('$', ''));
			return numberedPrice >= 1000000 || numberedPrice <= 0;
		} else {
			return true;
		}
	}

	async function fetchBulkPricingData(variantIds, showTable = false) {
		if (showTable) setDisplayBulkPricingData("load");
		const response = await fetch(`/apps/pricing-api/bulk-pricing?variant_ids=${variantIds.join(',')}`);
		if (!response.ok) {
			setDisplayBulkPricingData("hide");
			throw new Error('Failed to fetch bulk pricing data');
		}
		if (showTable) setDisplayBulkPricingData("show");
		const responseJSON = await response.json();
		if (responseJSON[0].quantity_breaks.length > 1) {
			setFoundBulkPricingData("loaded");
		} else {
			setFoundBulkPricingData("unavaible");
		}

		return responseJSON;
	}

	const findExactPrice = () => {
		if(!!props.variantExactPricing) {//[{ id: 123, price: 1.1234}]
			const matchingVariant =  props.variantExactPricing.find(variantExactPrice => variantExactPrice.id.toString() === variant.id.toString());

			return !!matchingVariant ? matchingVariant.price : (props.b2bPrice ?? variant.price) + '00';
		}

		return null;
	}

	/********************
		Handler functions:
			handle bulk price table
	********************/

	async function handleBulkPricing(e) {
		e.preventDefault();
		e.stopPropagation()
		if (displayBulkPricingData === "load") return; // don't run if still loading
		const variant_id = e.target.dataset.variant_id;

		if (bulkPricingData) { // if data exists don't load again, just show/hide
			displayBulkPricingData === "show" ? setDisplayBulkPricingData("hide") : setDisplayBulkPricingData("show");
		} else {
			setBulkPricingData(await fetchBulkPricingData([variant_id], true));
		}
	}

	async function handleDetailsOpen(e) {
		setDetailsOpen(!detailsOpen);

		if (currentSiteFeatures.bulk_pricing && !bulkPricingData) {
			const variant_id = e.currentTarget.dataset.variant_id;
			setBulkPricingData(await fetchBulkPricingData([variant_id]));
		}
	}

	async function handleSignIn(e) {
		e.preventDefault();
		e.stopPropagation();

		document.querySelector(e.target.dataset.modal).show();
	}

  return (
    <div className="variant-row">
			<div className="variant-row-overview" data-variant_id={variant.id} onClick={handleDetailsOpen}>
				<div className="variant-row-item variant-row-item-sku">
					<a href={variantUrl}>{variant.sku || "Item"}</a>
				</div>
				{variant.option1 && <div className="variant-row-item variant-row-item-option">{variant.option1}</div>}
				{variant.option2 && <div className="variant-row-item variant-row-item-option">{variant.option2}</div>}
				{variant.option3 && <div className="variant-row-item variant-row-item-option">{variant.option3}</div>}
				<div className="variant-row-item variant-row-item-price plp-price-container">
					{force_sign_in ? (
						<><a data-modal="#login-prompt-modal" href={window.routes.account_login_url} onClick={handleSignIn}>{plpLocales.compact_sign_in_link}</a> {plpLocales.compact_sign_in_text}</>
					) : (
						<VariantRowPriceData variant={variant} b2bPrice={props.b2bPrice} fetchB2BPricing={props.fetchB2BPricing} bulkPricingData={bulkPricingData} quantity={quantity} unitOfMeasure={unitOfMeasure} notInYourCatalog={notInYourCatalog} findExactPrice={findExactPrice} />
					)}
				</div>
				<div className={classnames("variant-row-open-details", detailsOpen && "open")}>
					<OpenVariantIcon />
				</div>
			</div>
			<VariantRowDetails 
				// Primary Data modals the component is built off of
				variant={variant}
				options={props.options}
				// extra variant data
				variantMetafields={variantMetafields}
				variantUrl={variantUrl}
				b2bPrice={props.b2bPrice}
				fetchB2BPricing={props.fetchB2BPricing}
				// VariantRow state variables
				quantity={quantity}
				detailsOpen={detailsOpen}
				foundBulkPricingData={foundBulkPricingData}
				displayBulkPricingData={displayBulkPricingData}
				bulkPricingData={bulkPricingData}
				setBulkPricingData={setBulkPricingData}
				setQuantity={setQuantity}
				// Handler Functions
				handleSignIn={handleSignIn}
				handleBulkPricing={handleBulkPricing}
				loadExactPricing={props.loadExactPricing}
				// Helper Functions
				notInYourCatalog={notInYourCatalog}
				findExactPrice={findExactPrice}
				fetchBulkPricingData={fetchBulkPricingData}
				// product-level data the variant componant needs
				isDefaultTitle={props.isDefaultTitle}
				productId={props.productId}
				productTitle={props.productTitle}
				productImageUrl={props.productImageUrl}
				attributes={props.attributes}
				// ResultRow state variables
				isExactPricingLoading={props.isExactPricingLoading}
				hasVariantExactPricingData={!!props.variantExactPricing} // this is product level because variantExactPricing contains all variant's exact pricing
			/>
			{(currentSiteFeatures.bulk_pricing && displayBulkPricingData === "show" && bulkPricingData) &&
				<BulkPriceTable bulkPricingData={bulkPricingData} setDisplayBulkPricingData={setDisplayBulkPricingData} />
			}
    </div>
  );
}

/*=========================================
=====     Expanded Variant Details    =====
=========================================*/

export const VariantRowDetails = (props) => {
	const options = props.options;
	const variant = props.variant;
	const variantMetafields = props.variantMetafields;

	const {
		cql_package_display,
		cql_low_inventory_quantity,
		cql_product_lead_time,
		cql_promo_messaging,
		cql_minimum_order_quantity
	} = variantMetafields;

	const soldOutThreshold = isNaN(cql_minimum_order_quantity) ? 1 : parseInt(cql_minimum_order_quantity);
	const unitOfMeasure = cql_package_display ? cql_package_display : null;
	
	const promoMessage = cql_promo_messaging ? cql_promo_messaging : props.attributes.mfield_cql_promo_messaging;
	const lowStockThreashold = !isNaN(cql_low_inventory_quantity) ? parseInt(cql_low_inventory_quantity): 10;
	const isLowStock = (variant.inventory_quantity < lowStockThreashold && variant.inventory_quantity > soldOutThreshold); // WHEN WE HAVE DATA: use variant's message threshold; if it doesn't exist then set this to false

	return (
		<div className={classnames("variant-details", props.detailsOpen && "open")}>
			<div className="variant-details-inner">
				<div className="variant-details-section variant-details-image">
					<div className={'image-wrapper' + (props.productImageUrl ? "" : " no-image")}>
						{props.productImageUrl &&
							<img
								alt={`${props.productTitle}-${variant.title}`}
								className="lazyload "
								data-aspectratio="1"
								data-sizes="auto"
								data-image
								src={props.productImageUrl}
							/>
						}
						{!props.productImageUrl &&
							<NoImage />
						}
					</div>
				</div>
				<div className="variant-details-section variant-details-info">
					<a href={props.variantUrl}>{variant.sku}</a>
					<a href={props.variantUrl}>{props.productTitle}</a>
					{!props.isDefaultTitle && 
						[1,2,3].map((i) => {
							return (variant[`option${i}`]) && <span>{options[(i - 1)].name}: {variant[`option${i}`]}</span>
						})
					}
					<a className="body-small" href={props.variantUrl}>{plpLocales?.viewDetails}</a>
				</div>
				<div className="variant-details-section variant-details-pricing">
					{force_sign_in ? (
						<ExpandedSignIn handleSignIn={props.handleSignIn} />
					) : (
						props.notInYourCatalog() ? (
							<VariantRowPriceData variant={variant} b2bPrice={props.b2bPrice} fetchB2BPricing={props.fetchB2BPricing} bulkPricingData={props.bulkPricingData} quantity={props.quantity} unitOfMeasure={unitOfMeasure} notInYourCatalog={props.notInYourCatalog} findExactPrice={props.findExactPrice} />
						) : <>
							<span className="plp-price-container">
								<VariantRowPriceData variant={variant} b2bPrice={props.b2bPrice} bulkPricingData={props.bulkPricingData} quantity={props.quantity} unitOfMeasure={unitOfMeasure} notInYourCatalog={props.notInYourCatalog} findExactPrice={props.findExactPrice} />
								{(currentSiteFeatures.exact_pricing && !props.hasVariantExactPricingData && !props.notInYourCatalog()) &&
									<ViewExactPricingCta
									loadExactPricing={props.loadExactPricing}
									isExactPricingLoading={props.isExactPricingLoading}
								/>}
							</span>
							{promoMessage && <span className="promo-message">{promoMessage}</span>} 
							{(isLowStock || cql_product_lead_time) &&
								<span className="shipping-qty-msgs">
									{/* Lead time Messaging */}
									{
										cql_product_lead_time && <>
											<ShippingIcon />
											<span>Ships in {cql_product_lead_time} {cql_product_lead_time == 1 ? 'day' : 'days'}</span>
										</>
									}
									{/* Low Stock Messaging */}
									{isLowStock && 
										<span className="js-qty-msg pdp__qty-msg pdp__show-qty-msg">
											{plpLocales?.lowStock /* WHEN WE HAVE DATA: this will be a variant metafield */ }
										</span>
									}
								</span>
							}
							{/* Buy in Bulk Messaging */}
							{currentSiteFeatures.bulk_pricing && (
								{
									"searching": <span className="loading-animation">
										<LoadingIcon />
									</span>,
									"loaded": <a href="#" className="shipping-qty-msgs" data-variant_id={variant.id} onClick={props.handleBulkPricing}>
										<BoxIcon />
										Buy in Bulk & Save
										{props.displayBulkPricingData === "load" && 
											<span className="loading-animation">
												<LoadingIcon />
											</span>
										}
									</a>
								}[props.foundBulkPricingData]
							)}
							{/* Add to Cart Form */}
							{
								variant.inventory_quantity >= soldOutThreshold ? (
									<VariantATCForm 
										variant={variant}
										productId={props.productId}
										quantity={props.quantity}
										setQuantity={props.setQuantity}
										bulkPricingData={props.bulkPricingData}
										setBulkPricingData={props.setBulkPricingData}
										fetchBulkPricingData={props.fetchBulkPricingData}
										findExactPrice={props.findExactPrice}
										minOrderQty={cql_minimum_order_quantity}
									/>
								) : (
									<div className="plp-atc">
										<span className="disabled button">{plpLocales.outOfStock}</span>
									</div>
								)
							}
						</>
					)}
				</div>
			</div>
		</div>
	)
}

/*=========================================
=====           Add to Cart           =====
=========================================*/

const VariantATCForm = (props) => {
	let bulkPricingData = props.bulkPricingData;
	const variant = props.variant;
	const quantity = props.quantity;
	const setQuantity = props.setQuantity;
	const minOrderQty = parseInt(props.minOrderQty) || 1;
	const maxOrderQty = Math.floor(variant.inventory_quantity / minOrderQty) * minOrderQty;

	const [quantityAtMin, setQuantityAtMin] = useState((quantity - minOrderQty) < minOrderQty); // boolean: true if qty can't be lowered
	const [quantityAtMax, setQuantityAtMax] = useState((quantity + minOrderQty) > maxOrderQty); // boolean: true if qty can't be raised
	const [submiting, setSubmiting] = useState(false);
	const [errorMessage, setErrorMessage] = useState(null);

	const headerCart = document.querySelector('cart-notification') || document.querySelector('cart-drawer');

	/********************
		Helper functions:
			set quantity value for both qty handlers while rounding to the increment
	********************/

	function setValue(newValue, increment = 1) {
		const roundedValue = Math.round(newValue / increment) * increment;
		const maximum = Math.floor(variant.inventory_quantity / increment) * increment;

		const windowedQty = Math.min(Math.max(roundedValue, increment), maximum);

		setQuantityAtMin((windowedQty - increment) < increment);
		setQuantityAtMax((windowedQty + increment) > maximum);
		setQuantity(windowedQty);
	}

	/********************
		Handler functions:
			handle qty plus & minus buttons
			handle focus off of qty field
			handle qty field change
			handle form submit
	********************/
	function handleClick(e) {
		e.preventDefault();
		const increment = e.target.name === 'plus' ? minOrderQty : (minOrderQty * -1);
		setValue(quantity + increment, minOrderQty);
	}

	function handleBlur(e) {
		setValue(parseInt(e.target.value), minOrderQty);
	}

	function handleChange(e) {
		setQuantity(parseInt(e.target.value));
	}

	async function handleSubmit(e) {
		e.preventDefault();
		if (submiting) return; // don't run if still submiting
		setSubmiting(true);

		const formData = new FormData(e.target);

		const qty = parseInt(formData.get('quantity'));
		formData.set('quantity', Math.max(Math.round(qty / minOrderQty) * minOrderQty, minOrderQty));

		if (headerCart) { // add data for post atc pop-up to render properly
			formData.append('sections', headerCart.getSectionsToRender().map((section) => section.id));
			formData.append('sections_url', window.location.pathname);
		}

		try {
			if (currentSiteFeatures.bulk_pricing) {
				if (!bulkPricingData) {
					bulkPricingData = await props.fetchBulkPricingData([variant.id]);
					props.setBulkPricingData(bulkPricingData);
				}
				if (bulkPricingData) formData.append('properties[__bulk_pricing]', JSON.stringify(bulkPricingData[0].quantity_breaks));
			}

		} catch (error) {
			console.error('Error fetching bulkPricingData:', error);
		}

		try {
			const companyId = window.Resources.searchspring.exact_pricing.user_company_id;

			if (currentSiteFeatures.exact_pricing) {
				let fourDigitPrice = props.findExactPrice();
				if (!fourDigitPrice) {
					const response = await fetch(`/apps/pricing-api/customer-product-pricing?current_company_id=${companyId}&product_ids=${props.productId}`, {
						method: 'GET',
						headers: {
								'Content-Type': 'application/json',
						},
					});
					const responseJSON = await response.json();
					fourDigitPrice = responseJSON?.[0]?.variants?.filter((vp) => vp.id == variant.id)[0]?.price;
				}
				console.log('fourDigitPrice', fourDigitPrice);
				if (fourDigitPrice) formData.append('properties[__four_decimal_price]', fourDigitPrice);
			}

		} catch (error) {
			console.error('Error fetching variantExactPrices:', error);
		}

		// set up basic config data
		const config = fetchConfig('javascript');
		config.headers['X-Requested-With'] = 'XMLHttpRequest';
		delete config.headers['Content-Type'];
		
		config.body = formData; // add variant form data to request

		const response = await fetch(`${window.routes.cart_add_url}`, config);
		const responseData = await response.json();

		setSubmiting(false);

		if (response.status !== 200) {
			setErrorMessage(responseData.message);
			return;
		}

		setErrorMessage(null);
		if (headerCart) {
			headerCart.renderContents(responseData);
		} else {
			window.location = window.routes.cart_url;
			return;
		}

	}

	return (
		<form className="plp-atc " onSubmit={handleSubmit}>
			{minOrderQty > 1 && <div class="increment-informer">{`${plpLocales.increment_tooltip_msg.replace('<span><<replace>></span>', minOrderQty).replace('<<replace>>', minOrderQty)}`}</div>}
			<div class="quantity tooltip-container">
				<button class={classnames("quantity__button", "no-js-hidden", quantityAtMin && "disabled")} name="minus" type="button" onClick={handleClick}>
					<MinusIcon />
				</button>
				<input class="quantity__input"
						type="number"
						name="quantity"
						min={minOrderQty}
						max={maxOrderQty}
						value={quantity}
						aria-label={plpLocales.qty}
						onChange={handleChange}
						onBlur={handleBlur}
					/>
				<button class={classnames("quantity__button", "no-js-hidden", quantityAtMax && "disabled")} name="plus" type="button" onClick={handleClick}>
					<PlusIcon />
				</button>
			</div>
			<input type="hidden" name="id" value={variant.id}/>
			<button type="submit" className="button button--primary">
				{submiting ? (
					<span className="loading-animation">
						<LoadingIcon />
					</span>
				) : (
					<ATCIcon />
				)}
			</button>
			{errorMessage && 
				<span className='atc-error-message'>{errorMessage}</span>
			}
		</form>
	)
}

/*=========================================
=====   Variant Row & Details Price   =====
=========================================*/

const VariantRowPriceData = (props) => {
	const variant = props.variant;
	const quantityBreaks = props.bulkPricingData[0]?.quantity_breaks;

	if (!props.b2bPrice && currentSiteFeatures.b2b_pricing) props.fetchB2BPricing();

	let quantityBreakPrice = variant.price;
	if (quantityBreaks?.length > 1) {
		quantityBreaks.forEach((item) => {
			if (props.quantity >= item.quantity) {
				quantityBreakPrice = item.price;
			}
		});
	}

	function calculatePrice() {
		return props.findExactPrice() ?? (
			props.b2bPrice ?? (
				currentSiteFeatures.b2b_pricing ? <span class="loading-price"></span> : (
					quantityBreakPrice
				)
			)
		)
	}

	return (
		<>
			<span className="variant-price">{(props.notInYourCatalog() && props.b2bPrice) ? <span>{plpLocales.not_in_catalog}</span> : <>${calculatePrice()}</>}</span>
			{(variant.compare_at_price && parseFloat(variant.compare_at_price) > parseFloat(variant.price)) && <span className="variant-compare-price">${variant.compare_at_price}</span>}
			{(props.unitOfMeasure && !props.notInYourCatalog()) && <><span className="divider">/</span>{props.unitOfMeasure}</>}
		</>
	)
}

/*=========================================
===== Sign In within Variant Details  =====
=========================================*/

const ExpandedSignIn = (props) => {
	const variant = props.variant;

	return (
		<div class="plp__price-login-wrap">
      <p>{plpLocales.sign_in_prompt}</p>
      <div class="plp__price-login-cta-wrap">
        <a data-modal="#login-prompt-modal" href={window.routes.account_login_url} class="button" onClick={props.handleSignIn}>{plpLocales.sign_in_button}</a>
        <span>
					<a href="/" title="">{plpLocales.learn_more}</a>
					<LinkArrowIcon />
        </span>
      </div>
    </div>
	)
}

/*=========================================
=====     Bulk Pricing Table Modal    =====
=========================================*/

const BulkPriceTable = (props) => {

	const currentVariantBulkPricing = props.bulkPricingData[0];

	document.addEventListener("click", function closeBulkTable() {
		document.removeEventListener('click', closeBulkTable);
		props.setDisplayBulkPricingData("hide");
	});

	function handleClose(e) {
		e.preventDefault();
		// document.removeEventListener('click', closeBulkTable);
		props.setDisplayBulkPricingData("hide");
	}

	return (
		<div className="bulk-table" onClick={(e) => e.stopPropagation()}>
			<div className="bulk-table-header">
				<strong>{plpLocales.bulkTitle}</strong>
				<a href='#' onClick={handleClose}>Close</a>
			</div>
			<table className="plp__bulk-pricing-table">
				{currentVariantBulkPricing.quantity_breaks.map((q_break) => {
					return (
						<tr>
							<td>{q_break.quantity}+</td>
							<td>${q_break.price}</td>
						</tr>
					)
				})}
			</table>
		</div>
	)
}

const getFourDecimalPrice = () => {
  const companyId = window.companyId;
  const productId = window.productId;
  const variantId = window.variantId;
  const input = document.getElementById("four_decimal_price");
  const btn = document.getElementById("addToCartButton");
  const loading = document.getElementById("addToCartButtonSpinner");

  if (window.companyId) {

    btn?.classList.add('loading');
    loading?.classList.remove('hidden');

    fetch(`/apps/pricing-api/customer-product-pricing?product_ids=${productId}&current_company_id=${companyId}`, {
      method: 'GET',
      headers: {
          'Content-Type': 'application/json',
      },
    })
    .then(response => response.json())
    .then(data => {
        window.cql__ExactPricesList = data[0].variants.reduce((consolidatedVariants, variant) => {consolidatedVariants[variant.id] = variant.price; return consolidatedVariants}, {});

        const fourDecimalPrice = window.cql__ExactPricesList[variantId];
        input.value = fourDecimalPrice;
        btn?.classList.remove('loading');
        loading?.classList.add('hidden');
    })
    .catch(error => {
        console.error('Error fetching pricing:', error);
    });
  }
}
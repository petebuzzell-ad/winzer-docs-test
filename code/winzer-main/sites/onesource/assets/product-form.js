if (!customElements.get('product-form')) {
  customElements.define('product-form', class ProductForm extends HTMLElement {
    constructor() {
      super();
      this.form = this.querySelector('form');
      this.form.querySelector('[name=id]').disabled = false;
      this.form.addEventListener('submit', this.onSubmitHandler.bind(this));
      this.cart = document.querySelector('cart-notification') || document.querySelector('cart-drawer');
      this.submitButton = this.querySelector('[type="submit"]');
      if (document.querySelector('cart-drawer')) this.submitButton.setAttribute('aria-haspopup', 'dialog');
      this.hideErrors = this.dataset.hideErrors === 'true';
    }

    async onSubmitHandler(evt) {
      evt.preventDefault();
      if (this.submitButton.getAttribute('aria-disabled') === 'true') return;

      this.handleErrorMessage();
      this.submitButton.setAttribute('aria-disabled', true);
      this.submitButton.classList.add('loading');
      this.querySelector('.loading-overlay__spinner').classList.remove('hidden');

      const formData = new FormData(this.form);
      const qty = parseInt(formData.get('quantity'));
      const min = parseInt(this.form.quantity.min);
      formData.set('quantity', Math.max(Math.round(qty / min) * min, min));

      const variantId = formData.get('id');
      const quantity = formData.get('quantity') || 1;

      try {
        const bulkPricingData = await this.fetchBulkPricingData([variantId]);
        const processedData = this.processApiResponse(bulkPricingData);
        await this.addToCartWithBulkPricing(variantId, quantity, processedData[variantId]);
      } catch (error) {
        console.error('Error adding item to cart:', error);
        this.handleErrorMessage(error.message);
      } finally {
        this.submitButton.classList.remove('loading');
        if (this.cart && this.cart.classList.contains('is-empty')) this.cart.classList.remove('is-empty');
        if (!this.error) this.submitButton.removeAttribute('aria-disabled');
        this.querySelector('.loading-overlay__spinner').classList.add('hidden');
      }
    }

    async fetchBulkPricingData(variantIds) {
      const response = await fetch(`/apps/pricing-api/bulk-pricing?variant_ids=${variantIds.join(',')}`);
      if (!response.ok) {
        throw new Error('Failed to fetch bulk pricing data, please try adding to the cart again.');
      }
      return response.json();
    }

    processApiResponse(apiResponse) {
      const bulkPricingData = {};
      apiResponse.forEach(variant => {
        bulkPricingData[variant.variant_id] = JSON.stringify(variant.quantity_breaks);
      });
      return bulkPricingData;
    }

    async addToCartWithBulkPricing(variantId, quantity, bulkPricingData) {
      const formData = new FormData(this.form);
      if (this.cart) {
        formData.append('sections', this.cart.getSectionsToRender().map((section) => section.id));
        formData.append('sections_url', window.location.pathname);
      }

      if (bulkPricingData) {
        formData.append('properties[__bulk_pricing]', bulkPricingData);
      }

      const config = fetchConfig('javascript');
      config.headers['X-Requested-With'] = 'XMLHttpRequest';
      delete config.headers['Content-Type'];
      config.body = formData;

      const response = await fetch(`${routes.cart_add_url}`, config);
      const responseData = await response.json();

      if (responseData.status) {
        publish(PUB_SUB_EVENTS.cartError, {
          source: 'product-form',
          productVariantId: variantId,
          errors: responseData.description,
          message: responseData.message
        });
        throw new Error(responseData.description || responseData.message);
      }

      if (!this.cart) {
        window.location = window.routes.cart_url;
        return;
      }

      publish(PUB_SUB_EVENTS.cartUpdate, {source: 'product-form', productVariantId: variantId});

      const quickAddModal = this.closest('quick-add-modal');
      if (quickAddModal) {
        document.body.addEventListener('modalClosed', () => {
          setTimeout(() => { this.cart.renderContents(responseData) });
        }, { once: true });
        quickAddModal.hide(true);
      } else {
        this.cart.renderContents(responseData);
      }
    }

    handleErrorMessage(errorMessage = false) {
      if (this.hideErrors) return;

      this.errorMessageWrapper = this.errorMessageWrapper || this.querySelector('.product-form__error-message-wrapper');
      if (!this.errorMessageWrapper) return;
      this.errorMessage = this.errorMessage || this.errorMessageWrapper.querySelector('.product-form__error-message');

      this.errorMessageWrapper.toggleAttribute('hidden', !errorMessage);
      if (errorMessage) {
        this.errorMessage.textContent = errorMessage;
      }
    }
  });
}

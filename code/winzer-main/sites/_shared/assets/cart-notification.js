class CartNotification extends HTMLElement {
  constructor() {
    super();

    this.notification = document.getElementById('cart-notification');
    this.header = document.querySelector('sticky-header');
    this.onBodyClick = this.handleBodyClick.bind(this);

    this.notification.addEventListener('keyup', (evt) => evt.code === 'Escape' && this.close());
    this.querySelectorAll('button[type="button"]').forEach((closeButton) =>
      closeButton.addEventListener('click', this.close.bind(this))
    );
  }

  open() {
    this.notification.classList.add('animate', 'active');

    this.notification.addEventListener('transitionend', () => {
      this.notification.focus();
      trapFocus(this.notification);
    }, { once: true });

    document.body.addEventListener('click', this.onBodyClick);
  }

  close() {
    this.notification.classList.remove('active');
    document.body.removeEventListener('click', this.onBodyClick);

    removeTrapFocus(this.activeElement);
  }

  renderContents(parsedState) {
      this.cartItemKey = parsedState.key;
      this.getSectionsToRender().forEach((section => {
        // document.getElementById(section.id).innerHTML =
        //   this.getSectionInnerHTML(parsedState.sections[section.id], section.selector);

        const elementToReplace = document.getElementById(section.id);

        const sectionInnerHtml = this.getSectionInnerHTML(parsedState.sections[section.id], section.selector);

        if(section.id === 'cart-icon-bubble') {
          /*
          replace header cart icon label html returned from api with html used
          in the liquid header icon links for labels - this prevents the label
          from not rendering after a cart update
          */
          const parser = new DOMParser();
          const headerCartLinkInnerHtml = parser.parseFromString(sectionInnerHtml, 'text/html').body;
          const headerCartIconLabel = headerCartLinkInnerHtml.querySelector('span.visually-hidden');

          if(!!headerCartIconLabel) {
            const headerCartIconLabelClone = headerCartIconLabel.cloneNode(true);
            headerCartIconLabelClone.classList.replace('visually-hidden', 'header__icon-text');

            const headerCartIconLabelWrapper = document.createElement('span');
            headerCartIconLabelWrapper.classList.add('header__icon-text-wrap');
            headerCartIconLabelWrapper.appendChild(headerCartIconLabelClone);

            headerCartLinkInnerHtml.replaceChild(headerCartIconLabelWrapper, headerCartIconLabel);
          }

          elementToReplace.innerHTML = headerCartLinkInnerHtml.outerHTML;
        } else {
          elementToReplace.innerHTML = sectionInnerHtml;
        }

      }));

      if (this.header) this.header.reveal();
      this.open();
  }

  getSectionsToRender() {
    return [
      {
        id: 'cart-notification-product',
        selector: `[id="cart-notification-product-${this.cartItemKey}"]`,
      },
      {
        id: 'cart-notification-button'
      },
      {
        id: 'cart-icon-bubble'
      }
    ];
  }

  getSectionInnerHTML(html, selector = '.shopify-section') {
    return new DOMParser()
      .parseFromString(html, 'text/html')
      .querySelector(selector).innerHTML;
  }

  handleBodyClick(evt) {
    const target = evt.target;
    if (target !== this.notification && !target.closest('cart-notification')) {
      const disclosure = target.closest('details-disclosure, header-menu');
      this.activeElement = disclosure ? disclosure.querySelector('summary') : null;
      this.close();
    }
  }

  setActiveElement(element) {
    this.activeElement = element;
  }
}

customElements.define('cart-notification', CartNotification);

document.addEventListener("DOMContentLoaded", function(event) {
  let quantityButtons = document.querySelectorAll(".quantity__button");
  let removeButtons = document.querySelectorAll(".cart-remove-button");

  let shippingMet = document.querySelector(".cart-message__shipping--met");
  let shippingNotMet = document.querySelector(".cart-message__shipping--not-met");
  let thresholdMessages = document.querySelector(".cart__threshold-messages");

  let amountAway = document.getElementById("amountAway");
  let amountNeeded = document.getElementById("amountNeeded").innerHTML;

  //Attach event listeners to quantity/remove buttons on cart page
  if(quantityButtons != null) {
    quantityButtons.forEach(button => {
      button.addEventListener("click", shippingUpdate);
    });
  }

  if(removeButtons != null) {
    removeButtons.forEach(button => {
      button.addEventListener("click", shippingUpdate);
    });
  }

  //If cart has been update, recalculate the shipping threshold total
  function shippingUpdate() {
    //For opacity animation
    amountAway.classList.add("opacity-transition");

    //Mutation observer for subtotal
    //That's when the cart.js has been updated and it is then safe to recalculate
    let cartSubTotal = document.querySelector(".js-contents.totals__container");
    let subTotalConfig = { attributes: true, childList: true, subtree: true };

    let subTotalCallback = (mutationList, subTotalObserver) => {
      $.ajax({
        type: 'GET',
        url: '/cart.js',
        cache: false,
        dataType: 'json',
        success: function(cart) {
          let newTotalPrice = formatPrice(cart.total_price);

          if(Number(newTotalPrice) >= Number(amountNeeded)) {
            shippingMet.classList.remove("hidden");
            shippingNotMet.classList.add("hidden");
          }
          else if(Number(newTotalPrice) < Number(amountNeeded) && Number(newTotalPrice) != 0) {
            let newCalcPrice = Number(amountNeeded) - Number(newTotalPrice);

            amountAway.innerHTML = newCalcPrice.toFixed(2);
            amountAway.classList.remove("opacity-transition");
            
            shippingNotMet.classList.remove("hidden");
            shippingMet.classList.add("hidden");
          }
          else if (Number(newTotalPrice) <= 0) {
            thresholdMessages.classList.add("hidden");
          }
        }
      });

      //Loop through and re-apply event listener(s) every time
      //You MUST do this because the whole HTML refreshes on change() and you lose the original event listener(s)
      let quantityButtons = document.querySelectorAll(".quantity__button");
      let removeButtons = document.querySelectorAll(".cart-remove-button");

      if(quantityButtons != null) {
        quantityButtons.forEach(button => {
          button.addEventListener("click", shippingUpdate);

          //if gift does not have FREE GIFT in the title and the liquid doesn't catch it, this will 
          if(button.style.pointerEvents) {
            let quantityHide = button.parentNode.parentNode.parentNode;
            quantityHide.style.visibility = "hidden";
          }
        });
      }
      
      if(removeButtons != null) {
        removeButtons.forEach(button => {
          button.addEventListener("click", shippingUpdate);
        });
      }
    };
  
    let subTotalObserver = new MutationObserver(subTotalCallback);
    subTotalObserver.observe(cartSubTotal, subTotalConfig);
  }

  //Format price
  function formatPrice(price) {
    let priceString = String(price);
    let newPrice = priceString.substring(0, priceString.length - 2) + "." + priceString.substring(priceString.length - 2);

    return newPrice;
  }

  // Watch for mutations, from Free Gift addition
  let target = document.querySelector('.content-for-layout');
  let observer = new MutationObserver(function(mutations) {
    mutations.forEach(function(mutation) {
      let quantityButtons = document.querySelectorAll(".quantity__button");
      let removeButtons = document.querySelectorAll(".cart-remove-button");

      if(quantityButtons != null) {
        quantityButtons.forEach(button => {
          button.addEventListener("click", shippingUpdate);
        });
      }
    
      if(removeButtons != null) {
        removeButtons.forEach(button => {
          button.addEventListener("click", shippingUpdate);
        });
      }
    });
  });

  let config = {
    attributes: true,
    childList: true,
    characterData: true
  };

  observer.observe(target, config);
});
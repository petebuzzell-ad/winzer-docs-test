import { h, Component, Fragment } from "preact";
import { observer } from "mobx-react";
import classnames from "classnames";

import { Autocomplete as LibraryAutocomplete } from "@searchspring/snap-preact-components";

import { Result } from "../Result/Result";
import { IconClose } from "../Icons/IconClose";

import "./autocomplete.css";

@observer
export class Autocomplete extends Component {
  render() {
    const controller = this.props.controller;
    const autocompleteStrings =
      window.Resources?.searchspring?.autocomplete ?? {};

    const resultsTitle = controller.store.state.input
      ? autocompleteStrings.recommendedProducts
      : autocompleteStrings.popularProducts;

    return (
      <LibraryAutocomplete
        controller={controller}
        input={controller.config.selector}
        hideFacets
        hideBanners
        termsTitle={autocompleteStrings.suggestions}
        trendingTitle={autocompleteStrings.topSearches}
        contentTitle={resultsTitle}
        resultsSlot={<AutocompleteResults />}
        noResultsSlot={<AutocompleteNoResults />}
        termsSlot={<AutocompleteTerms />}
      />
    );
  }
}

// For reference, you can see where this slot component gets used by the library Autocomplete component here:
// https://github.com/searchspring/snap/blob/main/packages/snap-preact-components/src/components/Organisms/Autocomplete/Autocomplete.tsx#L503
const AutocompleteResults = ({ results, contentTitle, controller }) => {
  const numberOfResultsToShow = 4; // magic number - could later be shopify setting driven
  const resultsToShow = results.slice(0, numberOfResultsToShow);
  const { state } = controller.store;

  const autocompleteStrings =
    window.Resources?.searchspring?.autocomplete ?? {};

  return (
    <>
      <div className="ss__autocomplete__results-wrap">
        {contentTitle && (
          <div
            className={
              "ss__autocomplete__title ss__autocomplete__title--content"
            }
          >
            <h5 className="h6">
              {contentTitle}
            </h5>
          </div>
        )}

        <div className="ss__autocomplete__results">
          {resultsToShow.map((result) => (
            <div className="ss__autocomplete__result">
              <Result result={result} 
                hideBadge 
                hideSwatches
                hideWishlist
                hideReviews
                query={controller.urlManager.urlState.query}
              />
            </div>
          ))}
        </div>
        {!!state.input && results.length > 0 && (
          <a
            className="button button--primary button--medium"
            href={state.url.href}
          >
            {autocompleteStrings.viewAllResults}
          </a>
        )}
      </div>
    </>
  );
};

const AutocompleteNoResults = () => (
  <p className="ss__autocomplete__no-result">
    {window.Resources?.searchspring?.autocomplete?.noResultsAutocomplete}
  </p>
);

// For reference, you can see where this slot component gets used by the library Autocomplete component here:
// https://github.com/searchspring/snap/blob/main/packages/snap-preact-components/src/components/Organisms/Autocomplete/Autocomplete.tsx#L402
const AutocompleteTerms = (props) => {
  const {
    terms,
    trending,
    termsTitle,
    trendingTitle,
    showTrending,
    valueProps,
    emIfy,
    onTermClick,
    controller,
  } = props;
  const { state } = controller.store;
  return (
    <>
      <span className="close-autocomplete" onClick={controller.handlers.document.click}>
        <IconClose />
      </span>
      <div className="ss__autocomplete__inner-wrapper">
        {terms.length > 0 ? (
            <>
              {termsTitle ? (
                <div className="ss__autocomplete__title ss__autocomplete__title--terms">
                  <h5 className="h6">
                    {termsTitle}
                  </h5>
                </div>
              ) : null}
              <div className="ss__autocomplete__terms__options">
                {terms.map((term) => (
                  <AutocompleteTerm 
                    valueProps={valueProps}
                    onTermClick={onTermClick}
                    term={term}
                    state={state}
                    emIfy={emIfy}
                  />
                ))}
              </div>
            </>
        ) : null}

        {showTrending ? (
          <>
            {trendingTitle ? (
              <div className="ss__autocomplete__title ss__autocomplete__title--terms">
                <h5 className="h6">
                  {trendingTitle}
                </h5>
              </div>
            ) : null}
            <div className="ss__autocomplete__terms__options">
              {trending.map((term) => (
                <AutocompleteTerm 
                  valueProps={valueProps}
                  onTermClick={onTermClick}
                  term={term}
                  state={state}
                  emIfy={emIfy}
                />
              ))}
            </div>
          </>
        ) : null}
      </div>
    </>
  );
};

const AutocompleteTerm = (props) => {
  const {
    valueProps,
    onTermClick,
    term,
    state,
    emIfy
  } = props

  return (
    <div
      className={classnames("ss__autocomplete__terms__option", {
        "ss__autocomplete__terms__option--active": term.active,
      })}
    >
      <a
        onClick={(e) => onTermClick && onTermClick(e)}
        href={term.url.href}
        {...valueProps}
        onFocus={() => term.preview()}
      >
        {emIfy(term.value, state.input)}
      </a>
    </div>
  )
}
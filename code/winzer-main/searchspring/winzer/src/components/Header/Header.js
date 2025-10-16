import { h, Fragment, Component } from "preact";
import { useMemo } from 'preact/hooks';
import { observer } from "mobx-react";
import { withStore } from "@searchspring/snap-preact-components";

import "./Header.css";

import { SearchInput } from "../SearchInput/SearchInput"


@withStore
@observer
export class Header extends Component {
  render() {
    const { pagination, search, custom } = this.props.store;

    useMemo(() => {
      if (search && search.query) {
        /* Updating the original page title because the original result count is mismatching with searchspring */
        const title = document.querySelector('title');
        title.textContent = `Search: ${pagination.totalResults} results found for "${search.query.string}"`;  
      }
    }, [pagination.totalResults])

    return (
      <header class={`ss-header-container ${pagination.totalResults > 1 ? 'ss-header-results-found' : 'ss-header-results-not-found'}`}>
        {pagination.totalResults > 1 ? (
          <>
            <p className="ss-results-heading">Search Results</p>
            <p class="ss-title ss-results-title h2">
              {`We found `}
              <span class="ss-results-count-total">{pagination.totalResults}</span>
              {` result${pagination.totalResults == 1 ? '' : 's'}`}
              {search?.query && (
                <span>
                  {` for `}
                  <span class="ss-results-query">&#8220;{search.query.string}&#8221;</span>
                </span>
              )}
            </p>
          </>
        ) : (
          pagination.totalResults === 0 && (
            <p class="ss-title ss-results-title ss-no-results-title h2">
              <span className="ss-results-heading">Search Results</span>
              {search?.query ? (
                <>
                  <span>
                    {window.Resources.searchspring.results.noResultsHeading} {' '}
                    <span class="ss-results-query">
                      &#8220;{search.query.string}&#8221;
                    </span>{' '}
                  </span>
                  <span class="ss-results-header-body">
                    Please try another keyword or part number.
                  </span>
                  <span className="ss-no-results-search">
                    <SearchInput />
                  </span>
                </>
              ) : (
                <span>No results found.</span>
              )}
            </p>
          )
        )}
      </header>
    );
  }
}
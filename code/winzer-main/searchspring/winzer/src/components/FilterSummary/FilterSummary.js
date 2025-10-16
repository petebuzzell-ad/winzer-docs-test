import { h, Fragment, Component } from "preact";
import { observer } from "mobx-react";
import {
  withStore,
  withController,
  Filter,
  useController,
} from "@searchspring/snap-preact-components";

@withStore
@withController
@observer
export class FilterSummary extends Component {
  render() {
    const { facets, filters, slideOutTriggered } = this.props.store;
    const controller = this.props.controller;
    const useControllerConst = useController();
    const removeAll = controller?.urlManager.remove("filter");
    const { urlManager } = useControllerConst;

    const clearAll = () => {
      urlManager.remove('filter').go()
    }

    return filters.length ? (
      <div class="ss-summary">
        <div class="ss-summary-container">
          <div class="ss-list ss-flex-wrap-center">

          {filters.map(filter => {
              return (
                  <Filter
                      key={JSON.stringify(filter)}
                      valueLabel={filter.label}
                      onClick={e => removeFilter(e, filter)}
                  />
              )
          })}

          {filters.length > 0 && (
              <div class="clear-all">
                  <button onClick={clearAll}>clear all </button>
              </div>
          )}

            {/* {filters &&
              filters.map((filter) => (
                <div class="ss-list-option">
                  <a {...filter.url.link} class="ss-list-link">
                    <span class="ss-summary-label">{filter.facet.label}:</span>{" "}
                    <span class="ss-summary-value">{filter.value.label}</span>
                  </a>
                </div>
              ))}

            <div class="ss-list-option ss-summary-reset">
              <a {...removeAll.link} class="ss-list-link">
                Clear All
              </a>
              <a class="ss-list-link">Clear All</a>
            </div> */}
          </div>
        </div>
      </div>
    ) : null;
  }
}

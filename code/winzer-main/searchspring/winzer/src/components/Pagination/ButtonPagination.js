import { h, Fragment, Component } from "preact";
import { useMemo } from 'preact/hooks'
import { observer } from "mobx-react";

import { withController } from "@searchspring/snap-preact-components";

@withController
@observer
export class ButtonPagination extends Component {
  render() {
    const controller = this.props.controller;
		const {
			store: { pagination },
		} = controller;

    const loadMore = () => pagination.next.url.go({ history: 'replace' })
    const paginationTotalItems = pagination.totalResults;
    const paginationPageSize = pagination.pageSize;
    let paginationCurrentPage = pagination.page;

    const displayPageSize = () => {
      if (pagination.current.number + 1 !== pagination.last.number || pagination.totalResults % pagination.pageSize == 0) {
       return pagination.pageSize
      } else {
        return pagination.totalResults % pagination.pageSize
      }
    }

    const displayLoadNumber = () => {
      return paginationPageSize * paginationCurrentPage;
    }

    const loadMoreNumber = useMemo(() => {
      // Either show page size or the remainder number of products if on second to last page
      return displayPageSize()
    }, [pagination.current.number, pagination.last.number, pagination.pageSize, pagination.totalResults])

    const currentLoadNumber = useMemo(() => {
      // Returns "Viewing X of X" number
      return displayLoadNumber()
    }, [paginationPageSize, paginationCurrentPage])

    return (
      <div class="text-center">
        <p class="ss__results__current-viewing">{window.Resources.searchspring.results.currentlyViewing} {currentLoadNumber} of {paginationTotalItems}</p>
        <button class="button button--primary" onClick={loadMore}>
          {window.Resources.searchspring.results.loadMore}
        </button>
      </div>
    );
  }
}
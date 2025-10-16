import { h, Fragment, Component } from "preact";
import { observer } from "mobx-react";

import { Select, withStore } from "@searchspring/snap-preact-components";

@withStore
@observer
export class SortBy extends Component {
  constructor(props) {
    super(props);
  }

  render() {
    const { sorting } = this.props.store;

    return (
      <Select
        className="ss-sort-by"
        label={window.Resources.searchspring.toolbar.sort}
        options={sorting.options}
        selected={sorting.current}
        onSelect={(e, selection) => {
          selection.url.go();
        }}
      />
    );
  }
}

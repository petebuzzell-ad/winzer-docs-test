import { h, Component } from "preact";
import { observer } from "mobx-react";
import { SearchIcon} from "../Icons/SearchIcon"

@observer
export class SearchInput extends Component {
  render() {
    const controller = this.props.controller;

    return (
      <div class="search-modal__form ss-header__search--desktop-search header__search--desktop" data-loading-text="Search">
        <form action="/search" method="get" role="search" class="search search-modal__form">
          <div class="field">
            <input class="search__input field__input"
              id="Search-In-Modal"
              type="search"
              name="q"
              value=""
              placeholder="Search by keyword, item or part #"
            />
            <label class="field__label" for="Search-In-Modal">Search</label>
            <input type="hidden" name="options[prefix]" value="last"/>
            <button class="search__button field__button" aria-label="Search">
             <SearchIcon/>
            </button>
          </div>
        </form>
      </div>
    );
  }
}
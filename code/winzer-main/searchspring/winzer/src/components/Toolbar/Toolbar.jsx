/** @jsx jsx */
import { h, Fragment, Component } from "preact";
import { observer } from "mobx-react";
import { useMemo } from 'preact/hooks';
import { jsx, css } from '@emotion/react';

import { filters } from '@searchspring/snap-toolbox';

import { SortBy } from "./SortBy";
import { SidebarContents } from "../Sidebar/Sidebar";
import { MediaQuery } from '../Helpers/MediaQuery'
import { FacetOptionsFooter } from './FacetDisplays'
import { FilterSummary } from "../FilterSummary/FilterSummary";

import {
  Button,
  Facet,
  FacetSlider,
  FacetGridOptions,
  FacetPaletteOptions,
  FacetListOptions,
  FacetHierarchyOptions,
  Slideout,
  withStore,
} from "@searchspring/snap-preact-components";

import { IconClose } from '../Icons/IconClose'

import './toolbar.css'

@withStore
@observer
export class Toolbar extends Component {
  constructor(props) {
    super(props)
    this.state = {
        activeKey: null,
        showViewResultsButton: false,
    }

    this.setActiveKey = this.setActiveKey.bind(this)
    this.handleOutsideClick = this.handleOutsideClick.bind(this)
    this.preventPropagation = this.preventPropagation.bind(this)
    this.handleCloseSidebar = this.handleCloseSidebar.bind(this)
  }

  handleCloseSidebar() {
    const mobileFiltersButton = document.querySelector('.ss__slideout__button')
    mobileFiltersButton.click()
    document.body.classList.remove('filter-toolbar-open');
  }

  setActiveKey(value) {
    this.setState({
        activeKey: value,
    })
  }

  preventPropagation(e) {
      e.stopPropagation()
  }

  handleOutsideClick() {
      this.setState({
          activeKey: null,
      })
  }

  componentDidMount() {
    this.props.store.facets.forEach(facet => {
        facet.collapsed = true
    })

    const toolbar = document.getElementById('toolbar-filters')
    const body = document.body
    const sticky = toolbar.offsetTop

    /* Handle sticky header */
    const toggle = () => {
        if (window.pageYOffset + 100 > sticky) {
            toolbar.classList.add('sticky')
            body.classList.add('show-header')
        } else {
            toolbar.classList.remove('sticky')
            body.classList.remove('show-header')
        }
    }
    window.onscroll = () => toggle()

    /* Handle outside click */
    toolbar.addEventListener('click', this.preventPropagation)
    window.addEventListener('click', this.handleOutsideClick)
  }

  componentWillUnmount() {
      /*Clean up event handlers */
      const toolbar = document.getElementById('toolbar-filters')
      toolbar.removeEventListener('click', this.preventPropagation)
      window.removeEventListener('click', this.handleOutsideClick)
  }
  render() {
    const {
      facets,
      filters,
    } = this.props.store;

    const inlineCount = this.props.store.data.merchandising?.content?.inline?.length ?? 0
    const resultCount = this.props.store.data.pagination.totalResults - inlineCount

    let leftColumnLayout = ""

    if(window.Resources.searchspring.toolbar.leftColumnLayout == 'false') {
      leftColumnLayout = ""
    } else {
      leftColumnLayout = window.Resources.searchspring.toolbar.leftColumnLayout;
    }

    return (
      <div class="ss-toolbar ss-toolbar-top" id="toolbar-filters">
        <div class={ leftColumnLayout ? ("toolbar-wrapper left-column-sticky") : ("toolbar-wrapper") }>
          <MediaQuery query="(max-width: 990px)">

            <Slideout
              buttonContent={slideoutButton(filters)}
            >
              <Fragment>
                <div class="slideout-content-wrapper slideout-filter-by">
                  <h3 class="CQL-h3">Filter</h3>
                  <div
                    class="slideout-content-wrapper slideout-close-button"
                    onClick={this.handleCloseSidebar}
                  >
                    <IconClose />
                  </div>
                </div>
                <SidebarContents />
              </Fragment>

              <div class={
                  this.state.showViewResultsButton
                      ? 'slideout-view-results slideout-view-results--active'
                      : 'slideout-view-results'
                  }
              >
                <button class="CQL-p2 button button--primary" onClick={this.handleCloseSidebar}>See {resultCount} results</button>
              </div>
            </Slideout>
            <SortBy />
          </MediaQuery>

          <MediaQuery query="(min-width: 989px)">
            { leftColumnLayout ? (
                <SortBy />
            ) : (
              <>
                <Slideout buttonContent={slideoutButton(filters)}>
                  <Fragment>
                    <div class="slideout-content-wrapper slideout-filter-by">
                      <h3 class="CQL-h3">Filter</h3>
                      <div
                        class="slideout-content-wrapper slideout-close-button"
                        onClick={this.handleCloseSidebar}
                      >
                        <IconClose />
                      </div>
                    <SidebarContents />
                    </div>
                  </Fragment>

                  <div class={
                    this.state.showViewResultsButton
                        ? 'slideout-view-results slideout-view-results--active'
                        : 'slideout-view-results'
                    }
                  >
                    <button class="button button--primary CQL-p2" onClick={this.handleCloseSidebar}>{window.Resources.searchspring.recommendations.filterSeeResults} ({resultCount})</button>
                  </div>
                </Slideout>
                <SortBy />
                </>
            )
            }
          </MediaQuery>
        </div>
      </div>
    );
  }
}

const slideoutButton = (filters) => {
  let filtersApplied = filters.length > 0 ? `(${filters.length})` : null;

  return (
    <Button
      style={{
        margin: "0 1rem",
        display: "block",
        width: "100%",
        boxSizing: "border-box",
        textAlign: "center",
      }}
      className="ss__button__filter-toggle"
    >
      {window.Resources.searchspring.toolbar.filter} <span className="ss__filters-applied">{filtersApplied}</span>
      <svg class="icon icon-filter" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 13 14">
        <path fill="currentColor" fill-rule="evenodd" d="M2.776 2.768a1.778 1.778 0 0 1 1.333 1.73 1.778 1.778 0 0 1-1.333 1.728v6.551a.444.444 0 1 1-.889 0v-6.55a1.778 1.778 0 0 1 0-3.459V1.222a.444.444 0 1 1 .89 0v1.546Zm-1.333 1.73c0 .49.398.888.889.888V5.4a.889.889 0 0 0 .889-.903.889.889 0 0 0-1.778 0Zm10.222-.192a1.778 1.778 0 0 1 1.333 1.73 1.778 1.778 0 0 1-1.333 1.728v5.013a.444.444 0 1 1-.889 0V7.764a1.778 1.778 0 0 1 0-3.458V1.222a.444.444 0 1 1 .889 0v3.084Zm-1.333 1.73c0 .49.398.888.888.888v.018a.889.889 0 0 0 .89-.907.889.889 0 0 0-1.778 0ZM8.554 9.887A1.778 1.778 0 0 0 7.22 8.16V1.222a.444.444 0 1 0-.89 0V8.16a1.778 1.778 0 0 0 0 3.457v1.16a.444.444 0 1 0 .89 0v-1.16a1.778 1.778 0 0 0 1.333-1.729Zm-1.778.89a.889.889 0 1 1 .889-.89.889.889 0 0 1-.889.903v-.014Z" clip-rule="evenodd"/>
      </svg>
    </Button>
  );
};

class ControlledFacet extends Component {
  constructor(props) {
      super(props)
      this.handleClick = this.handleClick.bind(this)
  }

  handleClick() {
      const { facet, activeKey, setActiveKey } = this.props
      setActiveKey(activeKey === facet.field ? null : facet.field)
  }

  render({ facet, activeKey, setActiveKey, className }) {
      if (facet.field === activeKey) {
        className += 'selected-facet'
      } else {
        facet.collapsed = true
      }

      let showFacet = true

      return showFacet ? (
        <div class={className} onClick={this.handleClick}>
          <Facet
              facet={facet}
              disableOverflow={true}
              optionsSlot={<OptionsSlot setActiveKey={setActiveKey} />}
          />
        </div>
      ) : null
  }
}

export class OptionsSlot extends Component {
  constructor(props) {
      super(props)
      this.handleClick = this.handleClick.bind(this)
      this.handleClose = this.handleClose.bind(this)
  }

  handleClick(e) {
      e.stopPropagation()
  }

  handleClose() {
      const { setActiveKey } = this.props
      setActiveKey(null)
  }

  getFacetDisplay(facet) {
      switch (facet.display) {
        case 'slider':
          return <FacetSlider facet={facet} />
        case 'grid':
          let columns = facet.label == "Size" ? 8 : 6
          return <FacetGridOptions facet={facet} values={facet.values} columns={8} />
        case 'palette':
          return <CustomFacetPaletteOptions facet={facet} />;
        case 'hierarchy':
          return <FacetHierarchyOptions values={facet.values} />
        default:
          return <FacetListOptions values={facet.values} />
      }
  }

  render({ facet }) {
      const facetDisplay = this.getFacetDisplay(facet)

      return (
          <div onClick={this.handleClick}>
              {facetDisplay}
              <FacetOptionsFooter onClose={this.handleClose} />
          </div>
      )
  }
}

const CustomFacetPaletteOptions = ({ facet }) => {
  const { values } = facet;
  const { field } = facet
  const shopifyFileURL = window.Resources.searchspring.shopifyURLs.fileURL.split('/placeholder')[0];

  const transformedValues = useMemo(() => {
    return values.filter(v => v.label);
  }, [values]);

  const facetCss = useMemo(() => {
    const styles = {};

    transformedValues.forEach(v => {
      let filterLabel = v.label.replaceAll(' ', '-').toLowerCase();
      let filterImage = shopifyFileURL + "/" + field + "__" + filterLabel + ".png";
      let background = `url("${filterImage}")`;

      styles[`& .ss__facet-palette-options__option__palette--${filters.handleize(filterLabel)}`] = {
        display: 'block',
        background: background,
      };
    });

    return [css(styles)];
  }, [transformedValues]);

  return <FacetPaletteOptions css={facetCss} values={transformedValues} columns={12} hideIcon gapSize='0px' />;
};
/** @jsx jsx */
import { h, Fragment, Component, createRef } from "preact";
import { observer } from "mobx-react";
import { useMemo } from 'preact/hooks';
import { jsx, css } from '@emotion/react';

import { filters } from '@searchspring/snap-toolbox';
import { MediaQuery } from '../Helpers/MediaQuery';
import classnames from 'classnames';

import { ArrowUp } from "../Icons/ArrowUp";
import { ArrowDown } from "../Icons/ArrowDown";

import {
  ThemeProvider,
  defaultTheme,
  FilterSummary,
  // Facet,
  FacetSlider,
  FacetGridOptions,
  FacetPaletteOptions,
  FacetListOptions,
  FacetHierarchyOptions,
  StoreProvider,
  withStore,
  withController,
  ControllerProvider,
} from '@searchspring/snap-preact-components';

import './sidebar.css'

@observer
export class Sidebar extends Component {
  render() {
    const store = this.props.controller.store;

    return (
      <ThemeProvider theme={defaultTheme}>
        <ControllerProvider controller={this.props.controller}>
          <StoreProvider store={store}>
            <SidebarContents />
          </StoreProvider>
        </ControllerProvider>
      </ThemeProvider>
    );
  }
}

@withController
@withStore
@observer
export class SidebarContents extends Component {
  constructor(props) {
    super(props);
    this.sidebarRef = createRef();
    this.state = {
      isOverflowing: false,
    };
  }

  componentDidMount() {
    this.checkOverflow();
  }

  checkOverflow = () => {
    if (this.sidebarRef.current) {
      const isOverflowing = true;
      this.setState({ isOverflowing }); // this, for some reason, is causing the 1st 5 Facets to open up. keeping this here for now to not spend more time right now
    }
  };

  render() {
    const {
      filters,
      facets,
    } = this.props.store;

    const leftColumnLayout = this.props.leftColumnLayout;

    // open the first five filter on desktop only
    const firstFiveFacets = facets.slice(0, 5);
    if (window.innerWidth > 990) {
      firstFiveFacets.forEach(facet => {
        facet.collapsed = false;
      });
    }

    return (
      <div
        ref={this.sidebarRef}
        className={leftColumnLayout ? 
          "ss-sidebar-container-left-column ss-sidebar-overflow" : 
          "ss-sidebar-container"}
      >
        <MediaQuery query="(min-width: 989px)">
          <h3 class="h5 ss-sidebar-container-left-column__headline">Filter</h3>
          <hr class="facet__sidebar-separator"/>
        </MediaQuery>

        <FilterSummary
          filters={filters}
          controller={this.props.controller}
          hideFacetLabel={true}
          disableStyles={true}
        />

        <div class='ss__facets'>
          {facets.map((facet, index) => {
            return facet.display === 'none' ? null : (
              <Fragment key={`${facet.label}-${index}`}>
                <Facet facet={facet} leftColumnLayout={leftColumnLayout} />
                <hr class="facet__sidebar-separator"/>
              </Fragment>
            );
          })}
        </div>
      </div>
    );
  }
}


@withController
@withStore
@observer
export class Facet extends Component {
  render (){
    const { facet, controller, store, leftColumnLayout } = this.props;
    const defaultOverflowLimit = 8;
    const facetClass = 'ss-options-'+facet.label.toLowerCase().replace(/[^a-zA-Z ]/g, '').replace(' ', '-');

    if (leftColumnLayout) {
      facet.overflow?.setLimit(defaultOverflowLimit);
    }

    const getFacetOptions = () => {
      switch(facet.display) {
        case 'list':
          return <FacetListOptions
            values={leftColumnLayout ? facet.refinedValues ?? facet.values : facet.values}
            disableStyles={true}
            className={facetClass}
            hideCount={true}
          />
        case 'palette':
          return <CustomFacetPaletteOptions
            facet={facet}
            values={leftColumnLayout ? facet.refinedValues ?? facet.values : facet.values}
            columns={4}
            gapSize={'8px'}
            disableStyles={true}
            className={facetClass}
          />;
        case 'hierarchy':
          return <FacetHierarchyOptions
            values={leftColumnLayout ? facet.refinedValues ?? facet.values : facet.values}
            className={facetClass}
            hideCount={true}
          />;
        case 'slider':
          return <FacetSlider facet={facet} />;
        case 'grid':
          return <FacetGridOptions
            values={leftColumnLayout ? facet.refinedValues ?? facet.values : facet.values}
            columns={4}
            gapSize={'8px'}
            disableStyles={true}
            className={facetClass}
            hideCount={true}
          />;
        default:
          return;
      }
    }


    return (
    facet && (
      <div
        id={`ss__facet--${facet.field}`}
        className={classnames(
          `ss__facet ss__facet--${facet.field}`,
          {
            'ss__facet--collapsed': facet.collapsed,
            'ss__facet--expanded': !facet.collapsed
          }
        )}
      >
        <div className={classnames(`ss__dropdown ss__facet__dropdown ${facet.collapsed ? '' : 'ss__dropdown--open'}`)}>

            <button className="ss__facet__dropdown__button button-reset"
              aria-expanded={!facet.collapsed}
              onClick={(e) => {
                e.preventDefault();
                facet.toggleCollapse();
              }}
              aria-label={`currently ${facet.collapsed ? 'collapsed' : 'open'} ${facet.label} facet dropdown`}
              role="heading"
              aria-level="3"
            >
                  {facet.label}
                  {facet.collapsed ? <ArrowDown/> : <ArrowUp/> }
            </button>

            <div class="ss__dropdown__content">
                <div aria-live="polite" aria-relevant="additions">
                {getFacetOptions()}
                </div>

                {(!!facet.overflow?.enabled && leftColumnLayout) && (
                    <button
                      className={classnames('ss__facet__show-more-less', 'link', 'button-reset')}
                      open={`${facet.overflow.remaining ? 'false' : 'true'}`}
                      onClick={() => {
                        facet.overflow.toggle();
                      }}
                      ariaLabel={`${facet.overflow.remaining ?  window.Resources.searchspring.toolbar.showMore : window.Resources.searchspring.toolbar.showLess} ${facet.label}`}
                    >
                        {facet.overflow.remaining ?  window.Resources.searchspring.toolbar.showMore : window.Resources.searchspring.toolbar.showLess}
                    </button>
                )}

            </div>
        </div>
      </div>
    ))
  }
}


const OptionsSlot = ({ facet, limit }) => {
  const { label } = facet;
  const labelClass  = facet.label;

  const optionHelper = useMemo(() => {
    const optionHelperJSONString = document.getElementById('optionHelpers')?.textContent;
    const optionHelpers = optionHelperJSONString ? JSON.parse(optionHelperJSONString) : {};
    return optionHelpers[label.toLowerCase()]?.valueHelper;
  }, [label]);

  const facetDisplay = useMemo(() => {
    switch (facet.display) {
      case 'grid': {
        const isSizes = facet.label === 'Size' ? true : false;
        const columns = isSizes ? 5 : 4;
        const className = `ss__facet-grid-options`;
        return <FacetGridOptions values={facet.values} columns={columns} className={className} />;
      }
      case 'palette':
        return <CustomFacetPaletteOptions facet={facet} />;
      case 'hierarchy':
        return <FacetHierarchyOptions values={facet.values} facet={facet} />;
      case 'slider':
        return <FacetSlider facet={facet} />;
      default:
        return <FacetListOptions values={facet.values} facet={facet} />;
    }
  }, [facet]);

  return (
    <>
      {facetDisplay}

      {optionHelper && <OptionHelper html={optionHelper} />}
    </>
  );
};

const CustomFacetPaletteOptions = ({ facet, values }) => {
  const { field } = facet;
  const { label } = facet;
  const shopifyFileURL = window.Resources.searchspring.shopifyURLs.fileURL.split('/placeholder')[0];
  let columns = label.includes('olor') ? 4 : 3;

  const transformedValues = useMemo(() => {
    return values.filter(v => v.label);
  }, [values]);

  const facetCss = useMemo(() => {
    const styles = {};

    transformedValues.forEach(v => {
      let extension = field === "variant_head_style" || field === "ss_generic_head_type" ? ".svg" : ".png";
      let filterLabel = v.label.replaceAll(' ', '-').toLowerCase();
      let filterImage = shopifyFileURL + "/" + field + "__" + filterLabel + extension;
      let background = `url("${filterImage}")`;
      
      styles[`& .ss__facet-palette-options__option__palette--${filterLabel}`] = {
        display: 'block',
        background: background,
      };
    });

    return [css(styles)];
  }, [transformedValues]);

  return <FacetPaletteOptions css={facetCss} values={transformedValues}  facet={facet} columns={columns} hideCount={true} hideIcon gapSize='0px' />;
};

const transformPaletteValues = (facet ) => {
  const { values } = facet;
  const { field } = facet;
  const { label } = facet;
  const shopifyFileURL = window.Resources.searchspring.shopifyURLs.fileURL.split('/placeholder')[0];
  let columns = label.includes('olor') ? 4 : 3;

  const transformedValues = values.filter(v => v.label);

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

  return {
    style: facetCss,
    values: transformedValues,
    columns: columns,
    hideIcon: true,
    gapSize:'0px'
  };
};

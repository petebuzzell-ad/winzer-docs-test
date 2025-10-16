import { h, Fragment, Component } from "preact";
import { useMemo } from 'preact/hooks'
import { observer } from "mobx-react";

import {
  Banner,
  //  Results as ResultsComponent,
  withStore,
  withController,
} from "@searchspring/snap-preact-components";

import { DesktopFilterSummary } from "../FilterSummary/DesktopFilterSummary";
import { FeaturedFilter } from "../FilterSummary/FeaturedFilter";
import { ProductCount } from '../FilterSummary/ProductCount';
import { Profile } from "../Profile/Profile";
import { ResultsComponent } from './ResultsComponent';
import { SidebarContents } from "../Sidebar/Sidebar";
import { Toolbar } from "../Toolbar/Toolbar";
import { MediaQuery } from '../Helpers/MediaQuery';

import "./results.css";

@withStore
@withController
@observer
export class Results extends Component {
  componentDidMount() {
    const templateWrapper = document.querySelector(".search-results-template-wrapper");

    if (templateWrapper) {
      templateWrapper.classList.add("some-results");
    }

    // if in page promo banner element exists move into results section
    const promoElements = document.querySelectorAll('.js-in-page-promo-banner');
    const targetElement = document.getElementById('js-promo-banner-target');

    promoElements.forEach((promoElement) => {
      if (!!targetElement) {
        promoElement.classList.remove('visually-hidden');
        promoElement.parentNode.removeChild(promoElement);
        targetElement.appendChild(promoElement);
      }
    })
  }

  render() {
    const { merchandising, results, pagination, filters } = this.props.store;
    const controller = this.props.controller;


    let leftColumnLayout = ""

    if (window.Resources.searchspring.toolbar.leftColumnLayout == 'false') {
      leftColumnLayout = ""
    } else {
      leftColumnLayout = window.Resources.searchspring.toolbar.leftColumnLayout;
    }

    return (
      <div class={(leftColumnLayout ? 'ss-results-left-column left-column-wrapper' : 'ss-results') + " page-width"}>
        <Banner content={merchandising.content} type="header" />
        <Banner content={merchandising.content} type="banner" />
        <div class="clear"></div>
        <FeaturedFilter />

        <div class="ss-results__content-wrap">

          <Profile name="results" controller={controller}>
            {leftColumnLayout ? (
              <SidebarContents leftColumnLayout={leftColumnLayout} />
            ) : (
              null
            )}
            <div class="ss-results__wrap">
              <div id="js-promo-banner-target"></div>

              <div className="ss-results__top-wrapper">
                <Toolbar />
                <ProductCount />
              </div>

              <ResultsComponent
                className=""
                controller={controller}
                results={results}
                filters={filters}
              />
            </div>

          </Profile>
        </div>
        <div class="clear"></div>
      </div>
    );
  }
}

@withController
@withStore
@observer
export class NoResults extends Component {
  componentDidMount() {
    const templateWrapper = document.querySelector('.search-results-template-wrapper')

    if (templateWrapper) {
      templateWrapper.classList.add('no-results')
    }
  }

  render() {
    const store = this.props.store;
    return null;
  }
}

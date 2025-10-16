import { h, Fragment, Component, createRef } from "preact";
import { observer } from "mobx-react";

import {
  Carousel,
  Recommendation,
  ControllerProvider,
  //  Result,
} from "@searchspring/snap-preact-components";

import { Result } from '../../Result/Result'
import { MediaQuery } from '../../Helpers/MediaQuery'

import { ChevronRight } from "../../Icons/ChevronRight";

// CSS
import './recs.css'

@observer
export class Recs extends Component {
  constructor(props) {
    super();

    this.wrapRef = createRef();
    const controller = props.controller;

    if (!controller.store.profile) {
      controller.init();
      controller.search();
    }
  }

  componentDidMount() {
    // add class to show seciton when there are results
    if(!!this.props.controller.store?.results?.length) {
        this.wrapRef.current?.closest('.recommendations').classList.add('has-results');
    }
  }

  render() {
    const controller = this.props.controller;
    const store = controller?.store;
    const arr = [...Array(9).keys()];

    const breakpoints = {
      0: {
        slidesPerView: 1.3,
        slidesPerGroup: 1,
        spaceBetween: 10
      },
      749: {
        slidesPerView: 2,
        slidesPerGroup: 2,
        spaceBetween: 10
      },
      990: {
        slidesPerView: 4,
        slidesPerGroup: 4,
        spaceBetween: 20
      },
    };

    if(!store.results.length) return null;

    return (
      <ControllerProvider controller={this.props.controller}>
      <div ref={this.wrapRef}>
        <MediaQuery query="(min-width: 750px)">
          <Recommendation
            controller={controller}
            pagination={true}
            hideButtons={false}
            prevButton={<ChevronRight/>}
            nextButton={<ChevronRight/>}
            breakpoints={breakpoints}
            loop={false}
            allowTouchMove={false}
            a11y={true}
          >
            {store.results.map((result) => (
              <Result result={result} hideWishlist></Result>
            ))}
          </Recommendation>
        </MediaQuery>

        <MediaQuery query="(max-width: 749px)">
          <Recommendation
            controller={controller}
            pagination={false}
            hideButtons={true}
            prevButton={<ChevronRight/>}
            nextButton={<ChevronRight/>}
            breakpoints={breakpoints}
            loop={false}
            allowTouchMove={true}
            a11y={true}
          >
            {store.results.map((result) => (
              <Result result={result} hideWishlist></Result>
            ))}
          </Recommendation>
        </MediaQuery>
      </div>
      </ControllerProvider>
    );
  }
}

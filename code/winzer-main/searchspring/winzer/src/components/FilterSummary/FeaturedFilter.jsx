/** @jsx jsx */
import { Component, h } from 'preact'
import { useMemo } from 'preact/hooks'
import { observer } from 'mobx-react-lite'
import { jsx, css } from '@emotion/react';

import { filters } from '@searchspring/snap-toolbox';

import Flickity from 'react-flickity-component'
import "../Flickity/flickity.css";

const flickityOptions = {
    initialIndex: 0,
    cellAlign: 'left'
}

import {
  Facet,
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

import { useController, Filter } from '@searchspring/snap-preact-components'

import './featuredfilter.css'

export const FeaturedFilter = () => {
    const controller = useController()

    const {facets} = controller.store
    const featuredFilter = window.Resources.collection_filter
    const filterImages = window.Resources.filter_images
    const shopifyFileURL = window.Resources.searchspring.shopifyURLs.fileURL.split('files/placeholder')[0]

    if (filterImages != undefined){
        const convertLabel = (label) => {
            return label.toLowerCase().replace(' ', '_')
        }
    
        return (
            <div class="featured-filter page-width">
            {/* // Check the window.Resources.collection_filter to pick up the featured_filter type
            // Loop over the facets values and find a field that contains the name of the featured_filter (cast to lowercase and concatenated spaces as underscores)
            // IFF you find a matching facet field value, THEN display that facet at the top of the page
            // See Sidebar.jsx for rendering facets with images */}
    
                <div class='ss__featured-filter-container ss__featured-filter-container--desktop small-hide'>
                    {facets.map(facet => {
                        return convertLabel(facet.label) !== convertLabel(featuredFilter) ? null : (
                            facet.values.map(val => {
                                const {label, url} = val;
                                const imageFileName = filterImages.find(image => image.includes(label.toLowerCase()))
                                const imageUrl = shopifyFileURL + imageFileName
                                return <CustomFeaturedFilter label={label} url={url} imageUrl={imageUrl} />
                            })
                        )
                    })}
                </div>
    
                <div class='ss__featured-filter-container ss__featured-filter-container--mobile medium-hide large-up-hide'>
                 <Flickity
                    options={flickityOptions}
                    >
                    {facets.map(facet => {
                        return convertLabel(facet.label) !== convertLabel(featuredFilter) ? null : (
                            facet.values.map(val => {
                                const {label, url} = val;
                                const imageFileName = filterImages.find(image => image.includes(label.toLowerCase()))
                                const imageUrl = shopifyFileURL + imageFileName
                                return <CustomFeaturedFilter label={label} url={url} imageUrl={imageUrl} />
                            })
                        )
                    })}
                    </Flickity>
                </div>
            </div>
        )
    }
}

const CustomFeaturedFilter = ({ label, url, imageUrl }) => {
    const isSelected = (window.location.href.includes(label) ? 'selected-filter' : '')
    return <a href={url.href} className={`ss__featured-filter ${isSelected}`}>
        <img src={imageUrl} />
        <span>{label}</span>
    </a>
  };
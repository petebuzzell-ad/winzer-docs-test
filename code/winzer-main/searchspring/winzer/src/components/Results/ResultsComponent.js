/** @jsx jsx */
import { h } from 'preact'
import { useMemo } from 'preact/hooks'

import { observer } from 'mobx-react-lite'
import { jsx, css } from '@emotion/react'
import classnames from 'classnames'

import {
    Banner,
    CacheProvider,
    InlineBanner,
    useTheme,
    useDisplaySettings,
} from '@searchspring/snap-preact-components'

import { ButtonPagination } from '../Pagination/ButtonPagination'
import { ResultRow } from '../ResultRow/ResultRow'

export function defined(properties) {
    const definedProps = {}

    Object.keys(properties).map(key => {
        if (properties[key] !== undefined) {
            definedProps[key] = properties[key]
        }
    })

    return definedProps
}

const CSS = {
    results: ({ columns, gapSize }) =>
        css({
            display: 'flex',
            flexDirection: `column`,
            gap: gapSize,
        }),
}

const BannerType = {
    HEADER: 'header',
    FOOTER: 'footer',
    LEFT: 'left',
    BANNER: 'banner',
    INLINE: 'inline',
}

const Layout = {
    GRID: 'grid',
    LIST: 'list',
}

const defaultBreakpointsProps = {
    0: {
        columns: 1,
    },
    370: {
        columns: 2,
    },
    990: {
        columns: 3,
    },
    1300: {
        columns: 4,
    },
}

export const ResultsComponent = observer(properties => {
    const globalTheme = useTheme()

    let props = {
        // default props
        results: properties.controller?.store?.results,
        columns: 4,
        gapSize: '4rem',
        layout: Layout.GRID,
        filters: properties.filters,
        breakpoints: defaultBreakpointsProps,
        // global theme
        ...globalTheme?.components?.results,
        // props
        ...properties,
        ...properties.theme?.components?.results,
    }

    const displaySettings = useDisplaySettings(props.breakpoints)
    if (displaySettings && Object.keys(displaySettings).length) {
        props = {
            ...props,
            ...displaySettings,
        }
    }

    const { disableStyles, className, layout, style, controller } = props
    const {
        store: { merchandising },
    } = controller

    const subProps = {
        result: {
            // default props
            className: 'ss__results__result',
            // global theme
            ...globalTheme?.components?.result,
            // inherited props
            ...defined({
                disableStyles,
            }),
            // component theme overrides
            theme: props.theme,
        },
        inlineBanner: {
            // default props
            className: 'ss__results__inline-banner',
            // global theme
            ...globalTheme?.components?.inlineBanner,
            // inherited props
            ...defined({
                disableStyles,
            }),
            // component theme overrides
            theme: props.theme,
        },
    }

    let results
    if (props?.columns > 0 && props?.rows > 0) {
        results = props.results.slice(0, props.columns * props.rows)
    } else {
        results = props.results
    }

    const styling = {}
    if (!disableStyles) {
        styling.css = [
            CSS.results({
                columns: layout == Layout.LIST ? 1 : props.columns,
                gapSize: props.gapSize,
            }),
            style,
        ]
    } else if (style) {
        styling.css = [style]
    }

    const pagination = controller.store.pagination
    const featuredSwatchObj = controller.store.config?.globals?.featuredSwatchObj


    return results?.length ? (
        <CacheProvider>

            <div {...styling} className={classnames('ss__results', className)}>
                {results.map(result =>
                    (() => {
                        switch (result.type) {
                            case BannerType.BANNER:
                                return (
                                    <InlineBanner
                                        key={result.uid}
                                        {...subProps.inlineBanner}
                                        banner={result}
                                        layout={props.layout}
                                    />
                                )
                            default:
                                return (
                                    <ResultRow
                                        key={result.uid}
                                        {...subProps.result}
                                        result={result}
                                        layout={props.layout}
                                        controller={controller}
                                        featuredSwatchObj={featuredSwatchObj}
                                        filters={props.filters}
                                    />
                                )
                        }
                    })()
                )}
            </div>
            {pagination.totalPages > 1 && pagination.current.number !== pagination.last.number && (
                <div class="center load-more-button ss-toolbar-col page-width">
                    <ButtonPagination pagination={pagination} />
                </div>
            )}

            <Banner content={merchandising.content} type="footer" />
        </CacheProvider>
    ) : null
})

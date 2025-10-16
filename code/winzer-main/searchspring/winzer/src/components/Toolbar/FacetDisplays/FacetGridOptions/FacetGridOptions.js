import { Fragment, h } from 'preact'
import { useMemo } from 'preact/hooks'

import classnames from 'classnames'
import { observer } from 'mobx-react-lite'

import { useTheme, useMediaQuery } from '@searchspring/snap-preact-components'

import { getImageObj } from '../../../Helpers/getImageObj'

import './FacetGridOptions.css'

export const FacetGridOptions = observer(properties => {
    const globalTheme = useTheme()
    const isMobile = useMediaQuery('(max-width: 991px)')

    const props = {
        values: [],
        ...globalTheme?.components?.facetGridOptions,
        ...properties,
        ...properties.theme?.components?.facetGridOptions,
    }

    const { facet, values, className } = props

    const { width, height, valuesWithImages } = useMemo(() => {
        const width = isMobile ? 110 : 120
        const height = isMobile ? 72 : 80
        const valuesWithImages = values.filter(value => !!getImageObj(facet.field, value.value))

        return { width, height, valuesWithImages }
    }, [facet.field, values, isMobile])

    return (
        valuesWithImages.length > 0 && (
            <div className={classnames('ss__facet-grid-options', className)}>
                {valuesWithImages.map(value => {
                    const imageObj = getImageObj(facet.field, value.value)
                    return (
                        <a
                            className={classnames('ss__facet-grid-options__option', {
                                'ss__facet-grid-options__option--filtered': value.filtered,
                            })}
                            aria-label={value.value}
                            {...value.url?.link}
                        >
                            <div>
                                <img
                                    src={imageObj.src}
                                    alt={imageObj.alt}
                                    width={width}
                                    height={height}
                                    loading="lazy"
                                />
                            </div>
                        </a>
                    )
                })}
            </div>
        )
    )
})

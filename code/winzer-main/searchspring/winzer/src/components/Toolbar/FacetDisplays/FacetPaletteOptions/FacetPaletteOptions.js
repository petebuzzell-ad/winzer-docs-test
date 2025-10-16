import { Fragment, h } from 'preact'
import { useMemo } from 'preact/hooks'

import classnames from 'classnames'
import { observer } from 'mobx-react-lite'

import { useTheme, useMediaQuery } from '@searchspring/snap-preact-components'

import { getImageObj } from '../../../Helpers/getImageObj'

import './FacetPaletteOptions.css'

export const FacetPaletteOptions = observer(properties => {
    const globalTheme = useTheme()
    const isMobile = useMediaQuery('(max-width: 991px)')

    const props = {
        values: [],
        ...globalTheme?.components?.facetPaletteOptions,
        ...properties,
        ...properties.theme?.components?.facetPaletteOptions,
    }

    const { facet, values, className } = props

    const { size, valuesWithImages } = useMemo(() => {
        const isColor = facet.field === 'color_family'
        const size = isColor ? 46 : isMobile ? 81 : 100
        const valuesWithImages = values.filter(value => !!getImageObj(facet.field, value.value))

        return { size, valuesWithImages }
    }, [facet.field, values, isMobile])

    return (
        valuesWithImages.length > 0 && (
            <div className={classnames('ss__facet-palette-options', className)}>
                {valuesWithImages.map(value => {
                    const imageObj = getImageObj(facet.field, value.value)
                    return (
                        <a
                            className={classnames('ss__facet-palette-options__option', {
                                'ss__facet-palette-options__option--filtered': value.filtered,
                            })}
                            aria-label={value.value}
                            {...value.url?.link}
                        >
                            <div>
                                <img
                                    src={imageObj.src}
                                    alt={imageObj.alt}
                                    width={size}
                                    height={size}
                                    loading="lazy"
                                />
                            </div>
                            <span className="ss__facet-palette-options__option__value">
                                {value.label}
                            </span>
                        </a>
                    )
                })}
            </div>
        )
    )
})

import { Fragment, h } from 'preact'
import { useMemo } from 'preact/hooks'

import classnames from 'classnames'
import { observer } from 'mobx-react-lite'
import { Dropdown, Icon, useMediaQuery, useTheme } from '@searchspring/snap-preact-components'

import { FacetOptionsFooter } from '../common/FacetOptionsFooter'

const facetField = 'size-width'
const facetLabel = 'size & width'

import './SizeWidthFacet.css'

import { ArrowDown } from '../../../Icons/ArrowDown';
import { ArrowUp } from '../../../Icons/ArrowUp';

export const SizeWidthFacet = observer(properties => {
    const isMobile = useMediaQuery('(max-width: 991px)')

    const props = {
        iconCollapse: 'angle-up',
        iconExpand: 'angle-down',
        sizeWidthFacetObject: {},
        ...properties,
    }

    const { isOpen, sizeWidthFacetObject, className } = props

    if (!Object.keys(sizeWidthFacetObject).length) {
        return null
    }

    return isMobile ? (
        <div class={isOpen ? 'facet-container facet-container--active' : 'facet-container'}>
            <SizeWidthContents {...props} />
        </div>
    ) : (
        <div className={classnames({ 'selected-facet': isOpen }, className)}>
            <SizeWidthContents {...props} includeFooter />
        </div>
    )
})

const SizeWidthContents = props => {
    const globalTheme = useTheme()
    const { isOpen, onClick, sizeWidthFacetObject, iconCollapse, iconExpand, includeFooter } = props

    return (
        <div className={classnames('ss__facet', `ss__facet--list`, `ss__facet--${facetField}`)}>
            <ArrowUp />
            <Dropdown
                open={isOpen}
                onClick={onClick}
                button={
                    <div className="ss__facet__header">
                        {facetLabel}
                        <ArrowDown />
                        {/* <Icon
                            {...{
                                className: 'ss__facet__dropdown__icon',
                                size: '15px',
                                ...globalTheme?.components?.icon,
                                theme: props.theme,
                            }}
                            icon={isOpen ? iconCollapse : iconExpand}
                        /> */}
                    </div>
                }
            >
                <div className="ss__facet__options">
                    <div className="size-width-subgroups">
                        <SizeWidthOptions
                            facet={sizeWidthFacetObject.littleKidsShoeSizeFacet}
                            maxLabelLength={4}
                        />
                        <SizeWidthOptions
                            facet={sizeWidthFacetObject.bigKidsShoeSizeFacet}
                            maxLabelLength={4}
                        />
                        <SizeWidthOptions facet={sizeWidthFacetObject.widthFacet} />
                    </div>

                    {includeFooter && <FacetOptionsFooter onClose={onClick} />}
                </div>
            </Dropdown>
        </div>
    )
}

const SizeWidthOptions = ({ facet, maxLabelLength }) => {
    const hideFacet = !facet?.values.length
    if (hideFacet) {
        return null
    }

    let facetValues = useMemo(() => {
        let cleanedValues = facet.values

        // Prevent broken size data from appearing
        if (maxLabelLength) {
            cleanedValues = cleanedValues.filter(value => value.label.length <= maxLabelLength)
        }

        // Prevent oddball widths from appearing
        if (facet.field === 'shoe_width') {
            const acceptableWidths = ['M', 'M/W', 'W', 'XW']
            cleanedValues = cleanedValues.filter(value => acceptableWidths.includes(value.value))
        }

        return cleanedValues
    }, [maxLabelLength, facet.values, facet.field])

    return (
        <div class="size-width-subgroup">
            <h3 class="size-width-subgroup__label">{facet.label}</h3>
            <div class="size-width-subgroup__options">
                {facetValues.map(value => (
                    <div class="ss__facet-option-wrapper">
                        <a
                            className={classnames('ss__facet-option', {
                                'ss__facet-option--filtered': value.filtered,
                            })}
                            aria-label={value.value}
                            {...value.url?.link}
                        >
                            <span className="ss__facet-option__value">{value.label}</span>
                        </a>
                    </div>
                ))}
            </div>
        </div>
    )
}

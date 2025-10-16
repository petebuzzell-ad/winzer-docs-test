/** @jsx jsx */
import { Component, h } from 'preact'
import { useMemo } from 'preact/hooks'
import { observer } from 'mobx-react-lite'
import { jsx, css } from '@emotion/react'

import { useController, Filter } from '@searchspring/snap-preact-components'

import './desktopfiltersummary.css'

export const DesktopFilterSummary = () => {
    const controller = useController()

    const { filters } = controller.store.data

    const { urlManager } = controller

    const removeFilter = (e, filter) => {
        const query = `filter.${filter.field}`
        urlManager.remove(query, filter.value).go()
    }

    const clearAll = () => {
        urlManager.remove('filter').go()
    }

    return (
        <div class="desktop-filter-summary">
            {filters.length > 0 && (
                <div class="clear-all">
                    <button onClick={clearAll}>Clear All</button>
                </div>
            )}
            <span class="active-filters">
                {filters.map(filter => {
                    return (
                        <Filter
                            key={JSON.stringify(filter)}
                            valueLabel={filter.label}
                            onClick={e => removeFilter(e, filter)}
                        />
                    )
                })}
            </span>
        </div>
    )
}

/** @jsx jsx */
import { useMemo } from 'preact/hooks'
import { jsx } from '@emotion/react'

import { useController } from '@searchspring/snap-preact-components'

import './productcount.css'

export const ProductCount = () => {
    const controller = useController()

    const inlineCount = useMemo(
        () => controller.store.data.merchandising?.content?.inline?.length ?? 0,
        [controller.store.data.merchandising?.content?.inline?.length]
    )

    const resultCount = useMemo(
        () => controller.store.data.pagination.totalResults - inlineCount,
        [controller.store.data.pagination.totalResults, inlineCount]
    )

    return (
        <div class="product-count">
            <div class="number-of-products">
                <div className="number-of-products__count body-special-large">{resultCount} Products</div>
            </div>
        </div>
    )
}

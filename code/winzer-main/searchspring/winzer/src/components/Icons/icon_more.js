import { Fragment, h } from 'preact'
export const IconMore = ({ count, breakpoint }) => {
    return (
        <div class="icon icon-more">
            +{count - breakpoint}
        </div>
    )
}

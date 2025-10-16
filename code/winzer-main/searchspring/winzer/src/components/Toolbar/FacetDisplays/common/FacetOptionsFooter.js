import { h } from 'preact'

import './FacetOptionsFooter.css'

export const FacetOptionsFooter = ({ onClose }) => {
    return (
        <div class="facet-options-footer">
            <hr />
            <button class="facet-options-footer__close" onClick={onClose}>
                CLOSE
            </button>
        </div>
    )
}

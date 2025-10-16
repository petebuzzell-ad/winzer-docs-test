import { h, Fragment, Component } from 'preact'

export class MediaQuery extends Component {
    constructor(props) {
        super(props)

        this.state = {
            matches: false,
        }
    }

    media
    handler

    componentDidMount() {
        const mediaQuery = window.matchMedia(this.props.query)
        this.setState({
            matches: mediaQuery.matches,
        })
        const handler = e => {
            this.setState({
                matches: e.matches,
            })
        }
        this.media = mediaQuery
        this.handler = handler

        mediaQuery.addEventListener('change', handler)
    }

    componentWillUnmount() {
        this.media.removeEventListener('change', this.handler)
    }

    render() {
        return this.state.matches ? this.props.children : null
    }
}

const chalk = require('chalk')

const log = (msg, ...rest) => console.log(chalk.green(msg), ...rest)
const error = (err, ...rest) => console.error(chalk.red(err), ...rest)

module.exports = {
    log,
    error
}

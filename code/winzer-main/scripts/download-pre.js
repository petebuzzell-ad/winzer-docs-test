const fs = require('fs-extra')

const {log, error} = require('./utils')

const { argv } = require("yargs")
    .scriptName("download-pre")
    .usage("Usage: $0")
    .example(
        "$0 -s striderite",
        "Downloads striderite code into ./download."
    )
    .option("s", {
        alias: "sitename",
        describe: "The site whose code you want to download",
        demandOption: "The sitename is required.",
        type: "string",
        nargs: 1,
    })

const { sitename } = argv

const downloadDir = './download'

log("Blowing away and recreating download directory...")

fs.removeSync(downloadDir)
fs.ensureDirSync(downloadDir)

fs.copy("./dawn", downloadDir)
    .then(() => {
        fs.copy("./sites/_shared", downloadDir)
            .then(() => {
                log('Copied shared files to download directory.')
                fs.copy(`./sites/${sitename}`, downloadDir)
                    .then(() => {
                        log(`Overlayed ${sitename} files onto download directory.`) 
                        log('Now downloading...')
                    })
                    .catch(error)
            })
            .catch(error)
        })
    .catch(error)


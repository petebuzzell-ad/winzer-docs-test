const fs = require('fs-extra')

const {log, error} = require('./utils')

var { argv } = require("yargs")
    .scriptName("deploy")
    .usage("Usage: $0 -s sitename")
    .example(
        "$0 -s striderite",
        "Deploys striderite code."
    )
    .option("s", {
        alias: "sitename",
        describe: "The site whose code you want to deploy",
        demandOption: "The sitename is required.",
        type: "string",
        nargs: 1,
    })

const { sitename } = argv;

const deployDir = './deploy_' + sitename;

log("Blowing away and recreating deploy directory...")

fs.removeSync(deployDir)
fs.ensureDirSync(deployDir)

fs.copy("./dawn", deployDir)
    .then(() => {
        log(`Copied Dawn files to ${deployDir} directory.`)
        fs.copy("./sites/_shared", deployDir)
        .then(() => {
            log(`Overlayed shared files to ${deployDir} directory.`)
            fs.copy(`./sites/${sitename}`, deployDir)
            .then(() => {
                log(`Overlayed ${sitename} files onto ${deployDir} directory.`) 
            })
            .catch(error)
        })
        .catch(error)
    })
    .catch(error)

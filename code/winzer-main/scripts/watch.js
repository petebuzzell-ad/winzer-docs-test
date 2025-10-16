const fs = require('fs-extra')
const path = require('path')
const Watchpack = require("watchpack")

const {log, error} = require('./utils')

var { argv } = require("yargs")
    .scriptName("watch")
    .usage("Usage: $0 -s sitename")
    .example(
        "$0 -s striderite",
        "Watches and deploys striderite code."
    )
    .option("s", {
        alias: "sitename",
        describe: "The site whose code you want to watch",
        demandOption: "The sitename is required.",
        type: "string",
        nargs: 1,
    })

const { sitename } = argv
const deployDir = './deploy_' + sitename;
const siteDir = `sites/${sitename}/`
const sharedDir = 'sites/_shared'

var wp = new Watchpack({
    aggregateTimeout: 1000,
	ignored:  "**/.git"
})

wp.watch({
	directories: [siteDir, sharedDir],
	startTime: Date.now() - 10000
})

wp.on("change", function(filePath) {
    const unwantedSitePath = path.normalize(siteDir + '/')
    const unwantedSharedPath = path.normalize(sharedDir + '/')
    
    const relPath = filePath.replace(unwantedSitePath, '').replace(unwantedSharedPath,'')
    
    // Check if the file updated was shared but overridden by the site
    if (filePath.includes(unwantedSharedPath) && fs.existsSync(path.join(siteDir, relPath))) {
        error(`Shared file updated, but ${sitename} is overriding this file here:`, path.join(siteDir, relPath))
        return
    }
    
    // Copy changed files into deploy directory
    log('updating for '+sitename)
    fs.copy(filePath, path.join(deployDir, relPath))
})

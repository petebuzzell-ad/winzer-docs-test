const fs = require('fs-extra')
const path = require('path')

const {log, error} = require('./utils')

const { argv } = require("yargs")
    .scriptName("download-post")
    .usage("Usage: $0")
    .example(
        "$0 -s striderite",
        "Decompose dowloaded code into shared and striderite directories."
    )
    .option("s", {
        alias: "sitename",
        describe: "The site whose code you want to download",
        demandOption: "The sitename is required.",
        type: "string",
        nargs: 1,
    })

const { sitename } = argv

const downloadDir = 'download'
const dawnDir = 'dawn'
const sharedDir = 'sites/_shared'
const siteDir = `sites/${sitename}`

function safeCopy(source, destination) {
    if (!fs.existsSync(path.dirname(destination))){
        fs.mkdirSync(path.dirname(destination), { recursive: true })
    }
    fs.copyFileSync(source, destination)
}

function moveFile(directory, file) {
    const fromPath = path.join(directory, file);

    fs.stat(fromPath, function (err, stat) {
        if (err) {
            error("Error stating file.", err)
            return;
        }

        if (stat.isFile()) {
            const destinationSubpath = path.join(directory,file).split(path.sep).slice(1).join(path.sep)
            const toSitePath = path.join(siteDir, destinationSubpath)
            const toSharedPath = path.join(sharedDir, destinationSubpath)
            const toDawnPath = path.join(dawnDir, destinationSubpath)
            
            if (fs.existsSync(toSitePath)) {
                const siteFileBuf = fs.readFileSync(toSitePath)
                const downloadedFileBuf = fs.readFileSync(fromPath)

                // Replace matching file in site directory if changed
                if (!downloadedFileBuf.equals(siteFileBuf)) {
                    log("File '%s' changed, updating there.", toSitePath)
                    safeCopy(fromPath, toSitePath)
                }
            } else if (fs.existsSync(toSharedPath)) {
                const sharedFileBuf = fs.readFileSync(toSharedPath)
                const downloadedFileBuf = fs.readFileSync(fromPath)

                // Replace matching file in shared directory if changed
                if (!downloadedFileBuf.equals(sharedFileBuf)) {
                    log("File '%s' changed, updating there.", toSharedPath)
                    safeCopy(fromPath, toSharedPath)
                }
            } else if (fs.existsSync(toDawnPath)) {
                const dawnFileBuf = fs.readFileSync(toDawnPath)
                const downloadedFileBuf = fs.readFileSync(fromPath)

                // If something changed in one of the non-modified Dawn files, add to site directory.
                if (!downloadedFileBuf.equals(dawnFileBuf)) {
                    log("Change to Dawn file detected. Creating site specific override '%s'", toSitePath)
                    safeCopy(fromPath, toSitePath)
                }
            } else {
                // Add new file to site directory
                log("Neither '%s' nor '%s' exists, copying to site directory", toSitePath, toSharedPath)
                safeCopy(fromPath, toSitePath)
            }
        }
        else if (stat.isDirectory()) {
            decomposeDirectory(fromPath)
        }
    });
} 

function decomposeDirectory(directory) {
    fs.readdir(directory, function (err, files) {
        if (err) {
            error("Could not list the directory.", err)
            process.exit(1)
        }

        files.forEach(function (file) {
            moveFile(directory, file)
        })
    })
}

decomposeDirectory(downloadDir)
const fs = require('fs-extra')
const path = require('path')

const {log, error} = require('./utils')

const { argv } = require("yargs")
    .scriptName("prune")
    .usage("Usage: $0")
    .example(
        "$0 -s striderite",
        "Remove any files from site that are identical to shared files."
    )
    .option("s", {
        alias: "sitename",
        describe: "The site whose code you want to prune",
        demandOption: "The sitename is required.",
        type: "string",
        nargs: 1,
    })

const { sitename } = argv

const sharedDir = 'sites/_shared'
const siteDir = `sites/${sitename}`

function pruneFile(directory, file) {
    const fromPath = path.join(directory, file);

    fs.stat(fromPath, function (err, stat) {
        if (err) {
            error("Error stating file.", err)
            return;
        }

        if (stat.isFile()) {
            const destinationSubpath = path.join(directory,file).split(path.sep).slice(2).join(path.sep)
            const sharedPath = path.join(sharedDir, destinationSubpath)
            
            if (fs.existsSync(sharedPath)) {
                const sharedFileBuf = fs.readFileSync(sharedPath)
                const siteFileBuf = fs.readFileSync(fromPath)

                // Remove site file if it matches a shared file exactly
                if (siteFileBuf.equals(sharedFileBuf)) {
                    log("'%s' matches shared file, deleting.", fromPath)
                    fs.rm(fromPath, () => {})
                }
            }
        }
        else if (stat.isDirectory()) {
            pruneDirectory(fromPath)
        }
    });
} 

function pruneDirectory(directory) {
    fs.readdir(directory, function (err, files) {
        if (err) {
            error("Could not list the directory.", err)
            process.exit(1)
        }

        files.forEach(function (file) {
            pruneFile(directory, file)
        })
    })
}

pruneDirectory(siteDir)
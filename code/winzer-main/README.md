# Shopify Propel

For instructions on how to use this template repo to set up new builds, see the [New Build](./NEW_BUILDS.md) docs.

This repo is a Dawn based, CQL-exclusive theme architecture intended to serve as a template for new shopify builds, particularly for multi-site clients. It is largely inspired by the cartridge architecture used in Salesforce Commerce Cloud - all sites ultimately use Shopify's Dawn theme as their base, with files selectively overridden either at the organization or site level.

In the `sites` directory, there is a `_shared` directory which houses the "core" dawn overrides that all sites share. Each individual site has its own dedicated site directory which can augment or override any of the files in the `_shared` directory.

- [First Time Setup](#first-time-setup)
  - [Cloning Repo and Submodule Initialization](#cloning-repo-and-submodule-initialization)
  - [Node](#node)
  - [Dependencies](#dependencies)
  - [Themekit](#themekit)
  - [Config Files](#config-files)
- [Usage](#usage)

## First Time Setup

### Cloning Repo and Submodule Initialization

Clone this repo with the following command, adding a local directory name if desired: `git clone --recurse-submodules https://github.com/cqlcorp/winzer.git <optional local directory name>`. The `--recurse-submodules` flag will automatically initialize and update the dawn submodule.

If you cloned the repo without this flag, simply run `git submodule init` and `git submodule update` in the root to properly initialize the dawn submodule.

### Node

We are currently using node 16.13.1 - use a node version manager for your OS if you need to juggle versions between projects: [Windows](https://github.com/coreybutler/nvm-windows) | [MacOS/unix](https://github.com/nvm-sh/nvm)

### Dependencies

Start by running `yarn` in the root (run `npm i -g yarn` if yarn is not yet installed) - this installs all dependencies for the project. You can safely ignore any manual-dependency installation warnings.

### Themekit

Themekit allows us to upload and download theme code from our local dev environments.

Follow its [instructions](https://shopify.dev/themes/tools/theme-kit/getting-started) for installation and requesting/establishing a password, proceeding to the next section in this readme once you have a password.

### Config Files

For each non-shared directory in `./sites`, add a `config.yml` file using the `config.template.yml` found there as a template. You will need to add your password from the Themekit Access App for each site.

In the admin of each shopify site you can duplicate the staging theme and rename it something like `<Your Name> Development`, which you can freely use to tinker around as needed. Once created, in the root of this project you can run a command like `yarn <site-name>:list` to see a list of all available themes for the site. You can then copy your new theme's id into the config file for the site. Running `yarn <site-name>:deploy` would then upload code to your new theme.

## Usage

Once config files are added for each site, the following npm scripts available to run in the project root:

- `<site>:list`: Lists the themes for the site
- `<site>:open`: Opens your development theme for the site
- `<site>:deploy`: Deploys your local code for the site to your development theme (update this id in your config file as needed)
- `<site>:watch`: Watches files for the site and deploys them your development theme
- `<site>:download`: Downloads the code from your development theme and updates local files appropriately
- `<site>:check`: Runs theme-check (linting) on your local code

Example: `yarn propel:list` would show a list of all available themes for a site named propel.

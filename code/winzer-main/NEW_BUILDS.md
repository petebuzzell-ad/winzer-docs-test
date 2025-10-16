# New Build Instructions

This [repo](https://github.com/cqlcorp/shopify-propel) is meant to serve as a template for new Shopify builds. When creating a new repo based off of this template, you'll need to do a few updates in order to prepare the architecture for your new site(s).

## Remove Propel

Since this repo is meant somewhat as a demo, it includes by default the `sites/propel` directory, which represents a fictitious bike store. This directory will roughly resemble what a real site-specific directory will look like, but includes branding and unique files specific to the demo store itself.

Hence, you can start by adding a sibling directory to `sites/propel` named after your new site: `sites/<your short store name>`. You can then copy and paste anything from the `sites/propel` directory you would like into your new directory, though this likely will amount to just the `config.template.yml` which should be updated to target your store's url and specific theme ids. You can then delete the `sites/propel` directory entirely (and any other sites that happen to be there, e.g. kanjam).

Note that the contents of the `_shared` directory represent brand-agnostic Propel overrides that should be retained and eventually edited to meet the needs of the new repo's collective sites.

## Updating NPM Scripts

The repo comes with NPM scripts useful for doing various code deployment actions, found in [package.json](./package.json).

By default, you'll only see scripts which target `propel` (and possibly other stores like kanjam). These can all be search-and-replaced with your site's directory's name from the Remove Propel step above (or deleted as need be). You can then invoke these scripts as seen in the [README](./README.md#usage).

## Updating CI/CD

The repo comes with an example of how Github Actions can be used to automate deployment to a staging theme (CD) and automate linting (via shopify theme check) on new PRs (CI). To use with your new build, you need to do the following:

1. In the new repo's settings (e.g. `https://github.com/cqlcorp/shopify-propel/settings/secrets/actions`), add (or ask the repo owner to add) the following secrets:
    1. `SHOPIFY_PASSWORD` - should be a new ThemeKit access pw dedicated solely to CI/CD. The tech lead should generally create this targeting their own email.
    1. `SHOPIFY_STORE_URL` - corresponds to the base url of the shopify store (e.g. `cql-demo.myshopify.com`).
    1. `SHOPIFY_THEME_ID` - corresponds to the theme id of your shopify store's staging theme.
    1. `THEME_PATH` - generally should be `./deploy` if you have not altered the build scripts.
1. If you have not already, create a `develop` branch off of your `main` branch and set it to be the default branch in the repo branch settings (e.g. `https://github.com/cqlcorp/shopify-propel/settings/branches`).
1. Update the ci.yml and cd.yml files in [.github/workflows](./.github/workflows) to target your site's directory's name instead of `propel`.

Once these steps are complete, you should see CI fire on new PRs and CD fire on merges/pushes to the `develop` branch.

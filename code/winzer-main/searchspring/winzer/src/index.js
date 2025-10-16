// Use if we want to enable chunks in the future
// eslint-disable-next-line no-undef
// __webpack_public_path__ = window.assetsPath;

import deepmerge from "deepmerge";

import { Snap } from "@searchspring/snap-preact";
import { getContext } from "@searchspring/snap-toolbox";
import { dynamicVariants } from './dynamic-variants.js';

import { afterStore } from "./middleware/plugins/afterStore";
import { configurable } from "./middleware/plugins/configurable";
import { combineMerge } from "./middleware/functions";
import { ContentSkel } from "./components/Content/Skel";
import { SidebarSkel } from "./components/Sidebar/Skel";



const context = getContext(["collection", "tags", "template", "shopper"]);

//Assign siteId based on the store
const stores = {
  onesource: {
    storeAddress: "winzeronesource.myshopify.com",
    siteId: "t047mf"
  },
  fastserv: {
    storeAddress: "winzerfastserv.myshopify.com",
    siteId: "wk4j0d"
  },
  corp: {
    storeAddress: "winzercorp.myshopify.com",
    siteId: "fsqw40"
  }
}
//Set a default for siteId
let siteId = stores.onesource.siteId;
//Get the current store
const currentStore = window.Shopify.shop;
//Set the actual siteId
switch (currentStore) {
  case stores.onesource.storeAddress:
    siteId = stores.onesource.siteId
    break;
  case stores.fastserv.storeAddress:
    siteId = stores.fastserv.siteId
    break;
  case stores.corp.storeAddress:
    siteId = stores.corp.siteId
    break;
}

const backgroundFilters = [];

if (context.collection?.handle) {
  // set background filters
  if (context.collection.handle != "all") {
    backgroundFilters.push({
      field: "collection_handle",
      value: context.collection.handle,
      type: "value",
      background: true,
    });
  }

  // handle collection tags (filters)
  if (context.tags) {
    var collectionTags = context.tags
      .toLowerCase()
      .replace(/-/g, "")
      .replace(/ +/g, "")
      .split("|");
    collectionTags.forEach((tag) => {
      backgroundFilters.push({
        field: "ss_tags",
        value: tag,
        type: "value",
        background: true,
      });
    });
  }
}

let dynamicVariantsConfig = {
  field: 'ss_swatches',
  limit: 4,
  swap: function(result, variant) {

      const core = result.mappings.core;
      const { attributes, custom } = result;

      if(variant.image) {
          core.imageUrl = variant.image;
      }

    custom.variantSelected = variant;
  }
};


/*
  configuration and instantiation
 */

let config = {
  url: {
    settings: {
      coreType: "query",
      customType: "query",
    },
    parameters: {
      core: {
        query: { name: "q" },
        page: { name: "p" },
      },
    },
  },
  client: {
    globals: {
      siteId: siteId,
    },
  },
  instantiators: {
    recommendation: {
      components: {
          Default: async () => {
              return (await import('./components/Recommendations/Recs/Recs')).Recs;
          },
      },
      config: {
          branch: 'develop',
      },
    },
  },
  controllers: {
    search: [
      {
        config: {
          id: 'search',
          plugins: [[afterStore], [configurable], [dynamicVariants, dynamicVariantsConfig]],
          settings: {
            redirects: {
              merchandising: true,
              singleResult: false
            },
            infinite: {},
            facets: {
              pinFiltered: false,
            },
          },
          globals: {
            filters: backgroundFilters,
            pagination: {
              pageSize: window?.ssConfig?.pageSize ?? 30,
            },
            ssConfig: window?.ssConfig,
          },
        },
        targeters: [
          {
            selector: '#searchspring-content',
            hideTarget: true,
            prefetch: true,
            component: async () => {
              return (await import('./components/Content/Content')).Content;
            },
          },
        ],
      },
    ],
    autocomplete: [
      {
        config: {
          id: 'autocomplete',
          selector: '.header__search-form .search__input',
          settings: {
            syncInputs: false,
            trending: {
              limit: 6,
              showResults: true,
            },
          },
          globals: {
            search: {
              query: {
                spellCorrection: true,
              },
            },
          },
        },
        targeters: [
          {
            selector: '.header__search-form .header__search-form-inner',
            component: async () => {
              return (await import("./components/Autocomplete/Autocomplete"))
                .Autocomplete;
            },
          },
        ],
      }
    ],
    finder: [
      {
        config: {
          id: "finder",
          url: "/",
          fields: [
            {
              field: "generic_color",
              label: "Color",
            },
          ],
        },
        targeters: [
          {
            name: "finder",
            selector: "#searchspring-finder",
            component: async () => {
              return (await import("./components/Finder/Finder")).Finder;
            },
          },
        ],
      },
      {
        config: {
          id: "finder_hierarchy",
          url: "/",
          fields: [
            {
              field: "ss_category_hierarchy",
            },
          ],
        },
        targeters: [
          {
            name: "finder_hierarchy",
            selector: "#searchspring-finder-hierarchy",
            component: async () => {
              return (await import("./components/Finder/Finder")).Finder;
            },
          },
        ],
      },
		],
    recommendation: [{
      config: {
        id: 'pdp-recently-viewed',
        tag: 'pdp-recently-viewed'
      }
    },
    {
      config: {
        id: 'pdp-customers-also-viewed',
        tag: 'pdp-customers-also-viewed'
      }
    },
    {
      config: {
        id: 'pdp-customers-also-bought',
        tag: 'pdp-customers-also-bought'
      }
    },
    {
      config: {
        id: 'cart-cross-sell',
        tag: 'cart-cross-sell'
      }
    },
    {
      config: {
        id: 'pdp-test-profile',
        tag: 'pdp-test-profile'
      }
    }
  ]
  },
};

// used to add config settings from cypress e2e tests
if (window?.mergeSnapConfig) {
  config = deepmerge(config, window.mergeSnapConfig, {
    arrayMerge: combineMerge,
  });
}

const snap = new Snap(config);

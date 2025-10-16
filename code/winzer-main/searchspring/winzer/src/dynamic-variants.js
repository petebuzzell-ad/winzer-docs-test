// This is an NPM Package, but this version has been modified to work with Brighton specifically.

let defaultConfig = {
    field: 'ss_swatches', // variant data field
    simple: 'color', // simple field in variant data (should be a string)
    group: 'group', // group field in variant data (can be an array or string)
    limit: 6, // number of variants to show
    filterEnable: true, // set to false to disable filter matching
    filterFields: ['color'], // array of filter fields to check
    searchEnable: true, // set to false to disable search matching
    swap: function(result, variant, event) {
        if (event?.stopPropagation) {
            // stopPropagation if applicable
            event.stopPropagation();
        }

        const core = result.mappings.core;
        const { attributes, custom } = result;

        // update product details
        if (variant.image) {
            core.thumbnailImageUrl = variant.image;
        }

        // set variantSelected
        custom.variantSelected = variant;
    },
    modifyVariant: false
};

let cache = {};
let currentFilters = [];
let searchMatch = false;

export const dynamicVariants = (controller, variantsConfig) => {
    variantsConfig = {...defaultConfig, ...variantsConfig};

    controller.on('afterStore', async ({ controller }, next) => {
        
        const store = controller.store;
        const { results, filters } = store;
        const query = store?.search?.query?.string;
        store.custom.variantsConfig = variantsConfig;

        if (results && results.length !== 0) {
            if (variantsConfig.filterEnable) {
                if (filters && filters.length !== 0) {
                    // reset filters on each afterStore
                    currentFilters = [];

                    // push filters to config for filter matching
                    filters.forEach((filter) => {
                        if (variantsConfig.filterFields.includes(filter.facet.field)) {
                            currentFilters.push(filter.value.label.toLowerCase());
                        }
                    });
                } else {
                    // reset filters if empty
                    currentFilters = [];
                }
            }

            if(variantsConfig.group && !Array.isArray(variantsConfig.group)) {
                variantsConfig.group = [variantsConfig.group];
            }

            results.forEach((result) => {
                if (result.type != 'banner') {
                    const core = result.mappings.core;
                    const { attributes, custom } = result;

                    // make variantSelected oberservable
                    result.custom = {...result.custom, variantSelected: {}};

                    // check if there are variants available
                    if (attributes[variantsConfig.field]) {
                        try {
                            // parse out the JSON for variant goodness
                            if (!cache[core.uid]) {
                                cache[core.uid] = JSON.parse(attributes[variantsConfig.field]);
                            }
                            attributes[variantsConfig.field] = cache[core.uid];


                            // loop through the variants
                            if (attributes[variantsConfig.field].length !== 0) {
                                attributes[variantsConfig.field].forEach((variant) => {
                                    if(variantsConfig.modifyVariant) {
                                        variantsConfig.modifyVariant(variant, result, controller);
                                    }

                                    // starting variant.weight
                                    variant.weight = 0;

                                    // set the simple and group data through the variant object
                                    let simpleData = variant[variantsConfig.simple] ? variant[variantsConfig.simple].toLowerCase() : '';
                                    
                                    let groupData = [];

                                    if(variantsConfig.group && variantsConfig.group.length !== 0) {
                                        variantsConfig.group.forEach((group) => {
                                            if(variant[group]) {
                                                if(typeof variant[group] == "string" || variant[group] instanceof String) {
                                                    variant[group] = [variant[group]];
                                                }

                                                groupData = groupData.concat(variant[group].map((data) => {
                                                    return data.toLowerCase();
                                                }));
                                            }
                                        });
                                    }
                                    
                                    // check simple and group data for matches in filters
                                    currentFilters.forEach((filter, i) => {
                                        let filterMultiplier = i + 1;

                                        if (simpleData) {
                                            variant.weight = calculateWeight(filter, simpleData, variant.weight, 10, filterMultiplier);
                                        }
                                        groupData.forEach((data) => {
                                            variant.weight = calculateWeight(filter, data, variant.weight, 5, filterMultiplier);
                                        });
                                    });

                                    // check search query for matches
                                    if (variantsConfig.searchEnable && query) {
                                        const terms = query.toLowerCase().split(' ');
                                        const termData = simpleData ? simpleData.replace('/', ' ').split(' ') : [];

                                        terms.forEach((term, i) => {
                                            let termMultiplier = i + 1;

                                            termData.forEach((data) => {
                                                variant.weight = calculateWeight(term, data, variant.weight, 1, termMultiplier);
                                                searchMatch = true;
                                            });
                                            groupData.forEach((data) => {
                                                variant.weight = calculateWeight(term, data, variant.weight, 1, termMultiplier);
                                                searchMatch = true;
                                            });
                                        });
                                    }
                                });

                                if ((currentFilters && currentFilters.length !== 0) || searchMatch) {
                                    // if there are filters or a search, sort the variants
                                    attributes[variantsConfig.field].sort((a, b) => {
                                        return b.weight - a.weight;
                                    });

                                    // use the first element in array (highest weight)
                                    if (attributes[variantsConfig.field][0].weight > 0) {
                                        variantsConfig.swap(result, attributes[variantsConfig.field][0]);
                                    }
                                } else {
                                    // remove variantSelected when no filters or search applied
                                    if (custom.variantSelected) {
                                        delete custom.variantSelected;
                                    }
                                }
                            }
                        } catch (error) {
                            controller.log.warn('error parsing variants:', error);
                        }
                    }
                }
            });
        }

        await next();
    });
};

// calculate weight for variants
function calculateWeight(value, criteria, weight, modifier, multiplier) {
    if (value == criteria || criteria.match(new RegExp('\\b'+value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')+'\\b', 'i'))) {
        weight += modifier * multiplier;
    }
    
    return weight;
}


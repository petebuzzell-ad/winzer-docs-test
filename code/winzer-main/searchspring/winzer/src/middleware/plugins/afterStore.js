export function afterStore(controller) {
  controller.on("init", async ({ controller }, next) => {
    controller.log.debug("initialization...");
    await next();
  });
  controller.on("afterStore", async ({ controller: { store } }, next) => {
    mutateFacets(store.facets);
    // mutateResults(store.results);

    await next();
  });

  controller.on("afterSearch", async (search, next) => {
    if (search?.response?.search?.query && search?.response?.pagination?.totalResults === 1 && !search?.response?.filters?.length) {
      const result = search?.response.results[0];
      const variants = JSON.parse(result.attributes.variants);
      const queryParam = variants.filter(v => v.sku.toLowerCase().indexOf(search?.response?.search?.query?.toLowerCase()) > -1).map(v => v.id)[0];

      let redirectURL = search?.response.results[0].mappings.core.url;
      if (queryParam) redirectURL = `${redirectURL}?variant=${queryParam}`;

      return window.location.replace(redirectURL);
    }

    await next();
  });

  // log the store
  // controller.on("afterStore", async ({ controller }, next) => {
  //   controller.log.debug("store", controller.store.toJSON());
  //   await next();
  // });

  // controller.on("afterStore", scrollToTop);
}

function mutateFacets(facets) {
  // for (let facet of facets) {
  //   let limit = 12;
  //   if (facet.display == "palette" || facet.display == "grid") {
  //     limit = 16;
  //   }

  //   facet.overflow?.setLimit(limit);
  // }
}

export async function scrollToTop(search, next) {
  // window.scroll({ top: 0, left: 0, behavior: "smooth" });
  // await next();
}

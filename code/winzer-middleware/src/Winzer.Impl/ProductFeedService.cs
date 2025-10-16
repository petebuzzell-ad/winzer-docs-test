using Winzer.Library;
using Winzer.Library.Salsify;
using Cql.Middleware.Library.Shopify.Products;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Dasync.Collections;
using CsvHelper;
using System.Globalization;
using Winzer.Core.Services;
using Winzer.Core.Types;
using Winzer.Library.Oracle;
using Cql.Middleware.Library.Shopify.Common;
using System.Collections.Generic;
using System.Collections;
using Cql.Middleware.Library.Shopify.Collection;
using Microsoft.Extensions.Configuration;
using Cql.Middleware.Library.Util;
using System.IO;
using System.Reactive.Joins;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net.Security;
using Amazon.S3.Model;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using Cql.Middleware.Library.Shopify.Catalog;
using Cql.Middleware.Library.Shopify.PriceList;

namespace Winzer.Impl
{
    public class ProductFeedService : IProductFeedService
    {
        private const string UPDATED_PRODUCTS_QUERY = "='salsify:product':valid(),'salsify:updated_at':gt('{0}')";
        private const string WINZER = "Winzer";
        private const string FASTSERV = "FastServ";
        private const string ONESOURCE = "OneSource";
        private readonly IShopifyProductService _shopifyService;
        private readonly IOracleProductTransmogrifier _transmogrifier;
        private readonly IShopifyProductMerger _shopifyProductMerger;
        private readonly IProductMapService _productMapService;
        private readonly IVariantMapService _variantMapService;
        private readonly ICollectionService _collectionService;
        private readonly ProductImportOptions _options;
        private readonly IFileService _sftpService;
        private readonly ICatalogService _catalogService;
        private readonly IPriceListService _priceListService;

        private readonly ILogger _logger;

        public ProductFeedService(IShopifyProductService shopify, IOracleProductTransmogrifier transmogrifier, IShopifyProductMerger shopifyProductMerger, ILogger<ProductFeedService> logger, IProductMapService productMapService, IVariantMapService variantMapService, ICollectionService collectionService, ProductImportOptions options, IFileService sftpService, ICatalogService catalogService, IPriceListService priceListService)
        {
            _shopifyService = shopify;
            _transmogrifier = transmogrifier;
            _shopifyProductMerger = shopifyProductMerger;
            _productMapService = productMapService;
            _variantMapService = variantMapService;
            _collectionService = collectionService;
            _options = options;
            _logger = logger;
            _sftpService = sftpService;
            _catalogService = catalogService;
            _priceListService = priceListService;
        }

        // DANGER! DANGER!  Do not run this unless you really want to delete all the products from Shopify!
        public async Task DeleteAllProducts()
        {
            Console.WriteLine($"Are you REALLY SURE you want to delete all the products from {_options.CompanyName}?");
            var response = Console.ReadLine();
            if (!"yes".Equals(response, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var productQuery = new ShopifyProductQuery()
            {
                IncludeImagesInResults = false,
                IncludeMetafieldsInResults = false,
                IncludeVariantMetafieldsInResults = false,
                IncludeVariantsInResults = false,
            };

            _logger.LogInformation("Getting all products from Shopify...");
            var allProducts = new List<ShopifyProduct>();
            string cursor = null;
            while (true)
            {
                var batch = await _shopifyService.GetProducts(productQuery, cursor);
                allProducts.AddRange(batch.Products);
                if (batch.HasMoreResults)
                {
                    cursor = batch.Cursor;
                }
                else
                {
                    break;
                }
            }

            int totalProducts = allProducts.Count;
            _logger.LogInformation($"Got {allProducts.Count} products from Shopify.");

            int counter = 0;
            foreach ( var product in allProducts )
            {
                try
                {
                    await _shopifyService.DeleteProduct(product.Id);
                }
                catch ( Exception e )
                {
                    _logger.LogError(e, "Failed to delete product!");
                }

                if (counter++ % 100 == 0)
                {
                    _logger.LogInformation($"Deleted {counter} of {totalProducts} products");
                }
            }

            _logger.LogInformation("Completed.");
        }

        /// <summary>
        /// Imports all products from Salsify into Shopify.  Not meant for normal day-to-day use, but will probably
        /// come in handy from time to time if significant logic changes are made that would require re-importing
        /// a lot of products when nothing was necessarily updated in Salsify.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ImportEverything(bool skipHashCheck = false)
        {
            Regex rgx = new Regex(_options.CompanyName + @".*?\.csv");
            var files = await _sftpService.ListDirectoryAsync("/Shopify/Data/Products");

            // only keep csv files and order filenames by timestamp from oldest to newest
            var csvFilePaths = files.Where((f) => rgx.IsMatch(f)).OrderBy((f) => DateTime.ParseExact(f.Substring(f.LastIndexOf("_") + 1, f.LastIndexOf(".")-f.LastIndexOf("_") - 1), "MMddyy", CultureInfo.InvariantCulture));

            var rval = true;
            foreach ( var filePath in csvFilePaths)
            {
                _logger.LogInformation("Processing file {0}", filePath);
                var allTheThings = await GetAllProductsFromOracleCSV(filePath);

                // Unlike with ImportUpdatedProducts, since we actually have ALL the data, we can make a true
                // determination about which products actually need to be updated right away
                var productsThatNeedToBeUpdated = new List<OracleCSVRecord>();
                foreach (var topLevelProduct in allTheThings)
                {
                    if (!CanIgnoreProductForImport(topLevelProduct, skipHashCheck))
                    {
                        productsThatNeedToBeUpdated.Add(topLevelProduct);
                    }
                }

                _logger.LogInformation($"Narrowed down list to {productsThatNeedToBeUpdated.Count()} products that actually need to be updated.");
                allTheThings = null; // Maybe this will help with memory utilization?

                var productReferenceProducts = new List<OracleCSVRecord>();
                var productCategories = new HashSet<string>();

                await productsThatNeedToBeUpdated.ParallelForEachAsync(async product =>
                {
                    try
                    {
                        await SendProductToShopify(product);

                        if (!string.IsNullOrEmpty(product.METRIC_VERSION) || !string.IsNullOrEmpty(product.IMPERIAL_VERSION))
                        {
                            productReferenceProducts.Add(product);
                        }

                        foreach (var category in product.WEBSITE_CATEGORY.Split('>'))
                        {
                            productCategories.Add(category);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send product with Oracle ID {product.ID} to Shopify.  Will continue on to the next product.");
                        rval = false;
                    }
                });

                //get all collections from shopify
                var existingCollections = await _collectionService.GetAllCollections();

                //create collections where product categories not in collection list
                var collectionsToCreate = productCategories
                    .Where(collectionName => !existingCollections.Collections.Any(ec => ec.title.ToLower() == collectionName.ToLower()))
                    .Where(collectionName => !string.IsNullOrWhiteSpace(collectionName))
                    .ToList();

                if (collectionsToCreate.Any())
                {
                    _logger.LogInformation($"Starting collection creation. There are {collectionsToCreate.Count()} collections that need to be created.");
                    //get categories metafield definition for the collection ruleset
                    var categoriesMetafieldID = await _collectionService.GetCategoriesMetafield();

                    if (categoriesMetafieldID != null)
                    {
                        await collectionsToCreate.ParallelForEachAsync(async collection =>
                        {
                            var collectionItem = new CollectionItem()
                            {
                                title = collection,
                                ruleSet = new CollectionRuleSet()
                                {
                                    appliedDisjunctively = false,
                                }
                            };

                            var collectionRule = new CollectionRule()
                            {
                                column = "PRODUCT_METAFIELD_DEFINITION",
                                conditionObjectId = categoriesMetafieldID,
                                relation = "EQUALS",
                                condition = collection,
                            };

                            collectionItem.ruleSet.rules.Add(collectionRule);

                            _logger.LogInformation($"Creating collection {collection}..");
                            try
                            {
                                await _collectionService.CreateCollection(collectionItem);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to create collection {0}", collection);
                            }
                        });
                    }
                    else
                    {
                        _logger.LogError($"Cannot create collections. Category metafield definition was not found. Expected metafield namespace and key is 'cql.categories'.");
                    }
                }
                else
                {
                    _logger.LogInformation($"No new collections need to be created.");
                }

                //wait until all products have been imported to update product reference metafields to ensure that the referenced products exist
                _logger.LogInformation($"Starting Product Reference Metafield Updates on {productReferenceProducts.Count()} products.");
                await productReferenceProducts.ParallelForEachAsync(async product =>
                {
                    try
                    {
                        //var watch = Stopwatch.StartNew();
                        await UpdateShopifyProductReferenceMetafields(product);
                        //watch.Stop();
                        //_logger.LogInformation($"Sent product {product.GetPropertyValueAsString("Product Name")} ({product.SalsifyId}) to Shopify in {watch.Elapsed}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send product with Oracle ID {product.ID} to Shopify.  Will continue on to the next product.");
                        rval = false;
                    }
                });

                // archive file
                var parts = filePath.Split('/').ToList();
                parts.Insert(parts.Count() - 1, "Archive");
                await _sftpService.MoveFileAsync(filePath, String.Join('/', parts.ToArray()));

                if (rval != true)
                {
                    _logger.LogError($"Errors occurred importing product feed {filePath}. The file will be archived, but this log should be reviewed to see if the file needs to be reprocessed.");
                }
            }

            return rval;
        }

        private bool CanIgnoreProductForImport(OracleCSVRecord product, bool skipHashCheck)
        {
            if (!skipHashCheck)
            {
                var computedHash = product.GetHash();
                if (computedHash == product.ShopifyHash)
                {
                    _logger.LogInformation($"Skipping product {product.ID} since the hash does not appear to have changed.");
                    return true;
                }
            }
            return false;
        }

        private async Task<IEnumerable<OracleCSVRecord>> GetAllProductsFromOracleCSV(string filePath)
        {
            var productList = new List<OracleCSVRecord>();
            var variantList = new List<OracleCSVRecord>();
            var brandId = getBrandId();

            _logger.LogInformation($"Loading product mappings for {brandId}...");
            var productMapDictionary = _productMapService.GetAllProductMaps(brandId).ToDictionary(m => m.OracleID, m => m);

            _logger.LogInformation($"Downloading CSV file...");
            var csvStream = await _sftpService.DownloadFileAsync(filePath);
            using var ms = new MemoryStream(csvStream);
            using var reader = new StreamReader(ms);
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                _logger.LogInformation($"Parsing CSV file...");
                var records = csv.GetRecords<OracleCSVRecord>();

                _logger.LogInformation($"Matching up CSV records to product mappings...");
                foreach (var record in records)
                {
                    if (productMapDictionary.ContainsKey(record.ID))
                    {
                        var map = productMapDictionary[record.ID];
                        record.ShopifyID = map.ShopifyID;
                        record.ShopifyHash = map.ProductHash;
                    }
                    // todo: update the product based on the record
                    if (!string.IsNullOrWhiteSpace(record.PARENT_PRODUCT_ID))
                    {
                        variantList.Add(record);
                    }
                    else
                    {
                        productList.Add(record);
                    }
                }
            }

            // Validate that we don't have any duplicate variant IDs.
            var variantSet = new HashSet<string>();
            var distinctVariants = new List<OracleCSVRecord>();

            foreach (var variant in variantList)
            {
                if (!variantSet.Add(variant.ID))
                {
                    _logger.LogWarning($"Found variaint {variant.ID} multiple times in CSV file.  Ignoring all occurrances after the first.");
                }
                else
                {
                    distinctVariants.Add(variant);
                }
            }
            variantList = distinctVariants;

            foreach(var product in productList)
            {
                product.Variants = variantList.Where(variant => variant.PARENT_PRODUCT_ID == product.ID).ToList();
            }

            var missingProductIDs = variantList
            .Where(v => !productList.Any(p => p.ID == v.PARENT_PRODUCT_ID))
            .Select(v => v.PARENT_PRODUCT_ID)
            .ToList();

            foreach (var id in missingProductIDs)
            {
                _logger.LogWarning($"Orphaned variants included in product feed. No parent product with Oracle ID {id} was found in the product feed.");
                //var productPlaceholder = new OracleCSVRecord();
                //productPlaceholder.ID = id;
                //productPlaceholder.Variants = variantList.Where(variant => variant.PARENT_PRODUCT_ID == id).ToList();
                //productList.Add(productPlaceholder);
            }

            _logger.LogInformation("Finished loading data from CSV file.");
            return productList;
        }

        private async Task UpdateShopifyProductReferenceMetafields(OracleCSVRecord oracleRecord)
        {
            // var updatedShopifyProduct = _transmogrifier.TransmogrifyToShopifyProduct(oracleRecord);
            var brandId = getBrandId();
            var map = _productMapService.GetProductMap(brandId, oracleRecord.ID);
            if (map == null)
            {
                _logger.LogWarning($"Ignoring reference metafields for product {oracleRecord.ID} as apparently it wasn't actually loaded.");
                return;
            }

            var unformattedId = map.ShopifyID;
            var formattedShopifyProductId = $"gid://shopify/Product/{map.ShopifyID}";

            // Check for Metric Version products links
            if (!string.IsNullOrWhiteSpace(oracleRecord.METRIC_VERSION))
            {
                var metricVersionProductMap = _productMapService.GetProductMap(brandId, oracleRecord.METRIC_VERSION);
                if (metricVersionProductMap != null)
                {
                    var metafield = new ShopifyMetafieldsSetInput()
                    {
                        OwnerId = formattedShopifyProductId,
                        Namespace = "cql",
                        Key = "metric_version",
                        Value = $"gid://shopify/Product/{metricVersionProductMap.ShopifyID}",
                        Type = "product_reference"
                    };

                    // Create/update metafield
                    await _shopifyService.SetMetafields(new ShopifyMetafieldsSetInput[] { metafield });
                }
                else
                {
                    _logger.LogWarning($"Could not find matching shopify product for Oracle Product ID {oracleRecord.METRIC_VERSION}");
                }                
            }

            // Check for Impreial Version product links
            if (!string.IsNullOrWhiteSpace(oracleRecord.IMPERIAL_VERSION))
            {
                var imperialVersionProductMap = _productMapService.GetProductMap(brandId, oracleRecord.IMPERIAL_VERSION);
                if (imperialVersionProductMap != null)
                {
                    var metafield = new ShopifyMetafieldsSetInput()
                    {
                        OwnerId = formattedShopifyProductId,
                        Namespace = "cql",
                        Key = "imperial_version",
                        Value = $"gid://shopify/Product/{imperialVersionProductMap.ShopifyID}",
                        Type = "product_reference"
                    };

                    // Create/update metafield
                    await _shopifyService.SetMetafields(new ShopifyMetafieldsSetInput[] { metafield });
                }
                else
                {
                    _logger.LogWarning($"Could not find matching shopify product for Oracle Product ID {oracleRecord.IMPERIAL_VERSION}");
                }
            }
        }

        /// <summary>
        /// Should be a private method, but made public so I could unit test it.
        /// </summary>
        /// <param name="salsifyProduct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task SendProductToShopify(OracleCSVRecord salsifyProduct)
        {
            // Change the import record into a shopify product.
            var brandId = getBrandId();
            var updatedShopifyProduct = _transmogrifier.TransmogrifyToShopifyProduct(salsifyProduct, brandId);
            var shopifyProductId = "";
            var shopifyProductHandle = "";
            var standaloneProductsToDelete = new List<string>();

            // Make sure we don't have multiple variants with the same SKU.
            var skuList = updatedShopifyProduct.Variants.Select(v => v.SKU);
            if (skuList.Count() != skuList.Distinct().Count())
            {
                var msg = $"Product {salsifyProduct.ID} has multiple variants with the same SKU.  Skipping this product.";
                _logger.LogError(msg);
                throw new Exception(msg);
            }

            // check if any variants of the current product were previously standalone products
            if (salsifyProduct.Variants.Any()) // if there are no variants for the product on the OracleCSVRecord, then this is a standalone product and we can skip the check
            {
                foreach (var variant in salsifyProduct.Variants)
                {
                    var map = _productMapService.GetProductMap(brandId, variant.ID);
                    if (map != null && !string.IsNullOrWhiteSpace(map.ShopifyID))
                    {
                        // If the variant product id was found in the product mapping table, it previously existed as a standalone product and should be cleaned up
                        standaloneProductsToDelete.Add(map.ShopifyID);
                    }
                }
            }
            else
            {
                // Make sure it has a weight if it doesn't have variants.
                if (string.IsNullOrWhiteSpace(salsifyProduct.WEIGHT) || string.IsNullOrWhiteSpace(salsifyProduct.WEIGHT_UNIT))
                {
                    _logger.LogWarning($"Skipping product {salsifyProduct.ID} as it has no weight and has no variants.");
                    return;
                }
            }


            if (!string.IsNullOrWhiteSpace(updatedShopifyProduct.Id))
            {
                // Get the original product and update it.
                var query = new ShopifyProductQuery()
                {
                    IncludeMetafieldsInResults = true,
                    IncludeVariantsInResults = true,
                    IncludeVariantMetafieldsInResults = true,
                    MetafieldNamespace = "cql",
                    IncludeImagesInResults = true,
                    ProductFields = new[] { "id", "title", "handle", "productType", "status", "descriptionHtml", "tracksInventory", "tags", "options { id name optionValues { id name } }" },
                    VariantFields = new[] { "id", "sku" },
                    ImageFields = new[] { "id", "url" }
                };

                var originalShopifyProduct = await _shopifyService.GetProduct(query, updatedShopifyProduct.Id);
                if (originalShopifyProduct != null)
                {
                    // Skip products that are already inactive if sendToShopify is false.
                    if (originalShopifyProduct.Status != "ACTIVE" && updatedShopifyProduct.Status != "ACTIVE")
                    {
                        _logger.LogInformation($"Skipping product {salsifyProduct.ID} since product is marked inactive and it is already archived in Shopify.");
                        return;
                    }

                    // Set status to archived if the original product was active, but salsify indicates
                    // that the product should no longer be sent to Shopify.
                    if (originalShopifyProduct.Status == "ACTIVE" && updatedShopifyProduct.Status != "ACTIVE")
                    {
                        // Just update the status on the original product since trying to actually update the
                        // product is likely to give a "could not update options to []" error.
                        _logger.LogInformation($"Archiving product {salsifyProduct.ID} - {originalShopifyProduct.Title} in Shopify as Status is no longer Active");
                        await _shopifyService.UpdateProductStatus(originalShopifyProduct.Id, "ARCHIVED");
                    }
                    else
                    {
                        // Merge in changes and update the product in Shopify.
                        var mergedProduct = _shopifyProductMerger.MergeProducts(originalShopifyProduct, updatedShopifyProduct);
                        _logger.LogInformation($"Updating product {salsifyProduct.ID} - {mergedProduct.Title} in Shopify.");
                        var result = await _shopifyService.UpdateProduct(mergedProduct);
                        UpdateVariantMapping(salsifyProduct, mergedProduct.Id, result.NewVariantsSKUToIdMap);
                    }
                }
                else
                {
                    throw new Exception($"Expected to find product with ID {updatedShopifyProduct.Id} in shopify, but didn't find anything.");
                }
            }
            else
            {
                if (updatedShopifyProduct.Options == null || !updatedShopifyProduct.Options.Any())
                {
                    _logger.LogWarning($"Product {salsifyProduct.ID} - {updatedShopifyProduct.Title} has no options!");
                    _logger.LogWarning(JsonConvert.SerializeObject(salsifyProduct, Formatting.Indented));
                }

                // Create the new product, but only if we have variants.
                if (updatedShopifyProduct.Variants != null && updatedShopifyProduct.Variants.Any())
                {
                    _logger.LogInformation($"Creating product {salsifyProduct.ID} - {updatedShopifyProduct.Title} in Shopify.");
                    //_logger.LogDebug(JsonConvert.SerializeObject(updatedShopifyProduct));
                    var result = await _shopifyService.CreateProduct(updatedShopifyProduct);
                    shopifyProductId = result.ProductId;
                    shopifyProductHandle = result.Handle;
                    UpdateVariantMapping(salsifyProduct, result.ProductId, result.VariantSKUToIdMap);
                    AddProductToMillionCatalog(result);
                }
                else
                {
                    _logger.LogInformation($"Skipping product creation for {salsifyProduct.ID} - {updatedShopifyProduct.Title} as it has no active variants.");
                }
            }
            
            if (!string.IsNullOrWhiteSpace(salsifyProduct.ID) && !string.IsNullOrWhiteSpace(shopifyProductId))
            {
                var map = _productMapService.GetProductMap(brandId, salsifyProduct.ID);
                if (map == null)
                {
                    map = new ProductMap
                    {
                        OracleID = salsifyProduct.ID,
                        ProductHash = salsifyProduct.GetHash(),
                        ShopifyID = RemoveGIDCrapFromShopifyId(shopifyProductId),
                        ShopifyProductHandle = shopifyProductHandle,
                        BrandId = brandId,
                    };
                    _productMapService.CreateProductMap(map);
                }
                else
                {
                    map.ProductHash = salsifyProduct.GetHash();
                    map.ShopifyID = RemoveGIDCrapFromShopifyId(shopifyProductId);
                    map.ShopifyProductHandle = shopifyProductHandle;
                    _productMapService.UpdateProductMap(map);
                }
            }

            // Clean up duplicate standalone products in shopify once variant versions have been successfully created
            foreach (var shopifyID in standaloneProductsToDelete)
            {
                // delete shopify product
                var formattedShopifyID = $"gid://shopify/Product/{RemoveGIDCrapFromShopifyId(shopifyID)}";
                await _shopifyService.DeleteProduct(formattedShopifyID);

                // remove entry from product mapping table
                _productMapService.DeleteProductMap(brandId, RemoveGIDCrapFromShopifyId(shopifyID));
            }
        }

        /// <summary>
        /// This method is a hack for Winzer and FastServ.  It adds any new products to a specific hard-coded catalog
        /// with a price of $1M.  This allows customers who can't purchase the product to still be able to see it, and
        /// front end logic looks for the $1M price point and shows messaging to the customer to indicate that they
        /// need to contact their franchisee for pricing.
        /// </summary>
        /// <param name="result"></param>
        /// <exception cref="NotImplementedException"></exception>
        private async void AddProductToMillionCatalog(ShopifyCreateProductResult result)
        {
            if (result.VariantSKUToIdMap == null || !result.VariantSKUToIdMap.Any()) return;

            var catalog = await _catalogService.GetCatalogByName("MILLION");
            if (catalog != null)
            {
                var variantsToAdd = result.VariantSKUToIdMap.Values.Select(id => new Tuple<string, decimal>(id, 1000000.0M));
                await _priceListService.UpsertVariantsToPriceList(catalog.PriceListId, "USD", variantsToAdd);
                await _catalogService.UpsertProductsToCatalogPublication(catalog.PublicationId, new string[] { result.ProductId });
            }
        }

        private void UpdateVariantMapping(OracleCSVRecord salsifyProduct, string shopifyProductId, IDictionary<string, string> variantDict)
        {
            if (shopifyProductId != null)
            {
                var brandId = getBrandId();
                foreach (var item in variantDict)
                {
                    string variantOracleID;
                    if (salsifyProduct.Variants.Count > 0)
                    {
                        variantOracleID = salsifyProduct.Variants.First(v => item.Key == v.PRIMARY_ITEM_NUMBER).ID;
                    } else
                    {
                        // Single-variant product - no variants specified in data feed
                        variantOracleID = salsifyProduct.ID;
                    }

                    if (!string.IsNullOrWhiteSpace(variantOracleID))
                    {
                        var variantMap = _variantMapService.GetVariantMap(brandId, variantOracleID);
                        if (variantMap == null)
                        {
                            variantMap = new VariantMap
                            {
                                OracleID = variantOracleID,
                                ShopifyVariantID = RemoveGIDCrapFromShopifyId(item.Value),
                                ShopifyProductID = RemoveGIDCrapFromShopifyId(shopifyProductId),
                                BrandId = brandId,
                            };
                            _variantMapService.CreateVariantMap(variantMap);
                        }
                        else 
                        {
                            if(variantMap.ShopifyVariantID != item.Value || variantMap.ShopifyProductID != shopifyProductId)
                            {
                                variantMap.ShopifyVariantID = RemoveGIDCrapFromShopifyId(item.Value);
                                variantMap.ShopifyProductID = RemoveGIDCrapFromShopifyId(shopifyProductId);
                                _variantMapService.UpdateVariantMap(variantMap);
                            }
                        }
                    }
                }
            }
        }

        public BrandEnum getBrandId()
        {
            var companyName = _options.CompanyName;
            BrandEnum? brand = null;

            switch (companyName)
            {
                case WINZER:
                    brand = BrandEnum.Winzer;
                    break;
                case FASTSERV:
                    brand = BrandEnum.FastServ;
                    break;
                case ONESOURCE:
                    brand = BrandEnum.OneSource;
                    break;
            }

            if (brand == null)
            {
                throw new Exception($"Configured company name ({companyName}) does not match any brand enum value.");
            }            

            return brand.Value;
        }

        public async Task<List<string>> GetAddOnProducts(SalsifyProduct salsify)
        {
            var addOnProducts = salsify.GetPropertyValueAsString("Add On Products Shopify IDs");
            var addOnShopifyProducts = new List<string>();
            if (!string.IsNullOrWhiteSpace(addOnProducts))
            {
                _logger.LogInformation($"Getting Add On Products {addOnProducts} for {salsify.SalsifyId}.");
                var productIds = addOnProducts.Split(',');
                foreach (var productId in productIds)
                {
                    var product = await _shopifyService.GetProduct(new ShopifyProductQuery(), productId);
                    if (product != null && !String.IsNullOrEmpty(product.Id))
                    {
                        addOnShopifyProducts.Add(product.Id);
                    }
                    else
                    {
                        _logger.LogWarning($"Product {salsify.SalsifyId} has invalid Add On Product Id {productId}. It could not be found in Shopify and will be skipped.");
                    }
                }
            }

            return addOnShopifyProducts;
        }

        /// <summary>
        /// Takes a string like "gid://shopify/ProductVariant/1341341343" and returns just "1341341343"
        /// </summary>
        /// <param name="idWithGidCrap"></param>
        /// <returns></returns>
        private static string RemoveGIDCrapFromShopifyId(string idWithGidCrap)
        {
            if (!string.IsNullOrEmpty(idWithGidCrap) && idWithGidCrap.StartsWith("gid"))
            {
                return idWithGidCrap[(idWithGidCrap.LastIndexOf('/') + 1)..];
            }
            return idWithGidCrap;
        }
    }
}

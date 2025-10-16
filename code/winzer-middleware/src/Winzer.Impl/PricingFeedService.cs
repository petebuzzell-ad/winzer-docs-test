using Cql.Middleware.Library.Oracle;
using Cql.Middleware.Library.Shopify.Catalog;
using Cql.Middleware.Library.Shopify.Company;
using Cql.Middleware.Library.Shopify.PriceList;
using Cql.Middleware.Library.Shopify.Products;
using Cql.Middleware.Library.Util;
using CsvHelper;
using Dasync.Collections;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Winzer.Core.Services;
using Winzer.Core.Types;
using Winzer.Library;

namespace Winzer.Impl
{
    public class PricingFeedService : IPricingFeedService
    {
        private const string FASTSERV = "FastServ";
        private const string WINZER = "Winzer";
        private const string ONESOURCE = "OneSource";
        private readonly IShopifyProductService _shopifyService;
        private readonly ICompanyService _companyService;
        private readonly ICatalogService _catalogService;
        private readonly IPriceListService _priceListService;
        private readonly IProductMapService _productMapService;
        private readonly IVariantMapService _variantMapService;
        private readonly IBulkPricingService _bulkPricingService;
        private readonly ILastPurchasePricingService _lastPurchasePricingService;
        private readonly ITemplatePricingService _templatePricingService;
        private readonly IContractPricingService _contractPricingService;
        private readonly PricingImportOptions _options;
        private readonly IFileService _sftpService;
        private readonly ILogger _logger;

        public PricingFeedService(
            IShopifyProductService shopify,
            ILogger<ProductFeedService> logger,
            IProductMapService productMapService,
            IBulkPricingService bulkPricingService,
            ITemplatePricingService templatePricingService,
            ICompanyService companyService,
            ICatalogService catalogService,
            IPriceListService priceListService,
            IVariantMapService variantMapService,
            IContractPricingService contractPricingService,
            ILastPurchasePricingService lastPurchasePricingService,
            PricingImportOptions options,
            IFileService sftpService)
        {
            _shopifyService = shopify;
            _productMapService = productMapService;
            _logger = logger;
            _companyService = companyService;
            _catalogService = catalogService;
            _priceListService = priceListService;
            _bulkPricingService = bulkPricingService;
            _templatePricingService = templatePricingService;
            _variantMapService = variantMapService;
            _contractPricingService = contractPricingService;
            _lastPurchasePricingService = lastPurchasePricingService;
            _sftpService = sftpService;
            _options = options;
        }

        private async Task<bool> ImportOneSourcePricing(IList<PricingCSVRecord> csvRecords, IDictionary<string, VariantMap> variantIdMap)
        {
            int recordCount = 0;
            int exceptionCount = 0;
            var productList = new List<string>();
            var variantPriceDictionary = new Dictionary<string, List<Tuple<string, decimal>>>();
            var dictLock = new object();

            // Build the variantPriceDictionary from the CSV records.
            await csvRecords.ParallelForEachAsync(async item =>
            {
                try
                {
                    if (recordCount % 1000 == 0)
                    {
                        _logger.LogInformation($"Processed {recordCount} of {csvRecords.Count()} records.");
                    }
                    Interlocked.Increment(ref recordCount);

                    if (string.IsNullOrWhiteSpace(item.VARIANT_ID))
                    {
                        _logger.LogWarning($"record has no variant id, ignoring record.");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(item.TEMPLATE_PRICE))
                    {
                        _logger.LogError($"Found OneSource price record with no template price for variant {item.VARIANT_ID}.  Skipping record.");
                        return;
                    }

                    if (!variantIdMap.ContainsKey(item.VARIANT_ID))
                    {
                        _logger.LogWarning($"Could not find variant mapping for variant {item.VARIANT_ID}.  This probably means the product hasn't been loaded to shopify yet.");
                        return;
                    }

                    var variantMap = variantIdMap[item.VARIANT_ID];
                    var price = Math.Ceiling(Decimal.Parse(item.TEMPLATE_PRICE) * 100) / 100; // Always round OneSource pricing.
                    UpsertBulkPricing(item, variantMap, price);

                    if (string.IsNullOrWhiteSpace(item.QUANTITY) || decimal.Parse(item.QUANTITY) == 0)
                    {
                        lock (dictLock)
                        {
                            // For non catalogs sort the variant data based on the product id.
                            if (!variantPriceDictionary.ContainsKey(variantMap.ShopifyProductID))
                            {
                                variantPriceDictionary.Add(variantMap.ShopifyProductID, new List<Tuple<string, decimal>>());
                            }
                            variantPriceDictionary[variantMap.ShopifyProductID].Add(new Tuple<string, decimal>(variantMap.ShopifyVariantID, price));
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception occurred while processing record #{recordCount}");
                    Interlocked.Increment(ref exceptionCount); ; // Maybe bail if we get too many exceptions?
                }
            });

            // Process the data by product.
            await variantPriceDictionary.ParallelForEachAsync(async entry =>
            {
                try
                {
                    var productId = entry.Key;
                    var variantsWithPrice = entry.Value.ToDictionary(t => t.Item1, t => t.Item2);
                    await _shopifyService.UpdateProductVariantPrices(productId, variantsWithPrice);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception occurred while updating product {entry.Key} in shopify.");
                    Interlocked.Increment(ref exceptionCount); ; // Maybe bail if we get too many exceptions?
                }
            });

            return exceptionCount == 0;
        }

        public async Task<bool> AddEverybodyToMillionCatalog()
        {
            _logger.LogInformation("Retrieving MILLION catalog");
            var catalog = await _catalogService.GetCatalogByName("MILLION");
            if (catalog == null)
            {
                _logger.LogError("MILLION catalog does not exist!");
                return false;
            }

            _logger.LogInformation("Getting all companies...");
            var companies = await _companyService.GetAllCompanies();
            _logger.LogInformation($"Got {companies.Companies.Count} companies.");

            int count = 0;
            await companies.Companies.ParallelForEachAsync(async company =>
            {
                var locations = await _companyService.GetLocationsForCompany(RemoveGIDCrapFromShopifyId(company.Id));
                var addResult = await _catalogService.AddCatalogToCompanyLocations(catalog.Id, locations.Select(l => l.Id));
                Interlocked.Increment(ref count);
                if (count % 100 == 0)
                {
                    _logger.LogInformation($"Processed {count} of {companies.Companies.Count} companies.");
                }
            });

            return true;
        }

        public async Task<bool> DeleteAllTheCatalogs()
        {
#if !DEBUG
    return false;
#endif
            Console.WriteLine("Are you REALLY SURE you want to delete all the catalogs?");
            var response = Console.ReadLine();
            if (!"yes".Equals(response, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            _logger.LogInformation("Retrieving all catalogs...");
            var allCatalogs = await _catalogService.GetAllCatalogs();
            _logger.LogInformation($"Got {allCatalogs.Count()} catalogs.");

            var recordCount = allCatalogs.Count();
            int processed = 0;
            await allCatalogs.ParallelForEachAsync(async catalog =>
            {
                Interlocked.Increment(ref processed);
                if (processed % 100 == 0)
                {
                    _logger.LogInformation($"Deleted {processed} of {recordCount} catalogs.");
                }

                if ((catalog.Title?.StartsWith("Company") ?? false) || (catalog.Title?.StartsWith("Contract") ?? false))
                {
                    await _catalogService.DeleteCatalog(catalog.Id);
                }
            }, 4);

            _logger.LogInformation("Complete.");

            return true;
        }

        public async Task<bool> ImportPricingData()
        {
            // we will be downloading a file for each company, options are:
            // OneSource
            // Winzer
            // FastServ
            // set the company id based on the file we are downloading. not sure if this will be determined by the file name or not
            var companyName = _options.CompanyName;

            BrandEnum? brand = GetDatabaseBrand(companyName);
            if (brand == null)
            {
                _logger.LogError($"Invalid value for CompanyName option: {_options.CompanyName}.  Exiting.");
                return false;
            }
            _logger.LogInformation($"Starting import for {companyName}");

            // Figure out if there are any files to process on the SFTP site.
            Regex rgx = new Regex(@".*?\.csv");
            var files = await _sftpService.ListDirectoryAsync("/Shopify/Data/Pricing");
            var csvFilePaths = files
                .Where((f) => rgx.IsMatch(f)).OrderBy((f) => DateTime.ParseExact(f.Substring(f.LastIndexOf("_") + 1, f.LastIndexOf(".") - f.LastIndexOf("_") - 1), "MMddyy", CultureInfo.InvariantCulture))
                .Where((f) => f.Contains(companyName));

            if (!csvFilePaths.Any())
            {
                _logger.LogInformation($"No appropriate files were found to process.  Exiting.");
                return true;
            }


            // Get variant id mapping data (WinzerID -> ShopifyID) that we'll use later.
            _logger.LogInformation("Loading variant mappings...");
            var variantMapDictionary = _variantMapService.GetAllVariantMaps(brand.Value).ToDictionary(i => i.OracleID ?? "", i => i);
            _logger.LogInformation("Finished loading variant mappings.");

            _logger.LogInformation($"Found the following files available for processing, will process them in order: {string.Join(',', csvFilePaths)}");

            bool atLeastOneFailure = false;
            foreach ( var filePath in csvFilePaths )
            {
                bool success = true;
                // Read in the CSV file.
                var allTheThings = await GetAllPricesFromCSV(companyName, filePath);

                if (companyName == ONESOURCE)
                {
                    success = await ImportOneSourcePricing(allTheThings, variantMapDictionary);
                }
                else
                {
                    success = await ImportWinzerOrFastServPricing(brand.Value, allTheThings, variantMapDictionary);
                }

                //archive csv
                var parts = filePath.Split('/').ToList();
                parts.Insert(parts.Count() - 1, "Archive");
                await _sftpService.MoveFileAsync(filePath, String.Join('/', parts.ToArray()));

                if (!success)
                {
                    atLeastOneFailure = true;
                }
            }

            return !atLeastOneFailure;
        }

        private async Task<bool> ImportWinzerOrFastServPricing(BrandEnum brand, IList<PricingCSVRecord> allTheThings, Dictionary<string, VariantMap> variantMapDictionary)
        {
            // Pull in all the company data from Shopify.  We'll need this later..
            var companyIdMap = new ConcurrentDictionary<string, string>(); // company external id as key, shopify company ID as value
            var contractToCompanyListMap = new Dictionary<string, List<string>>(); // contract ID as key, list of company external ids using that contract as value.
            var templateToCompanyListMap = new Dictionary<string, List<string>>(); // template name as key, list of company external ids using that template as value.
            await PopulateCompanyMappings(contractToCompanyListMap, templateToCompanyListMap, companyIdMap);
            _logger.LogInformation($"Found {contractToCompanyListMap.Count} contracts for companies");

            var productList = new List<string>();
            var listLock = new object();

            // Sort the pricing data into dictionaries and create all the records in our AWS pricing database.
            var catalogToVariantPriceMap = new ConcurrentDictionary<string, ConcurrentBag<Tuple<string, decimal>>>(); // catalogName as key, tuple of variantID and price as value
            var catalogToProductMap = new ConcurrentDictionary<string, ConcurrentBag<string>>(); // catalogName as key, list of shopify product ids as value.
            var templateCatalogs = new ConcurrentDictionary<string, bool>(); // names of template catalogs.
            var lastPurchaseCatalogs = new ConcurrentDictionary<string, string>(); // catalog name as key, external company id as value
            var contractCatalogs = new ConcurrentDictionary<string, string>(); // catalog name as key, contract id as value.
            int recordCount = 0;
            int exceptionCount = 0;
            await allTheThings.ParallelForEachAsync(async item =>
            {
                try
                {
                    Interlocked.Increment(ref recordCount);
                    if (recordCount % 1000 == 0)
                    {
                        _logger.LogInformation($"Processed {recordCount} of {allTheThings.Count()} records.");
                    }

                    if (string.IsNullOrWhiteSpace(item.VARIANT_ID))
                    {
                        //_logger.LogWarning($"item has no variant id, ignoring record.");
                        return;
                    }

                    // Look for the oracle variant id in our variant map.  If we can't find it, that means the variant hasn't been
                    // created in Shopify yet, so skip this record.
                    VariantMap? variantMap = null;
                    if (!variantMapDictionary.ContainsKey(item.VARIANT_ID))
                    {
                        //_logger.LogWarning($"Could not find variant mapping for variant {item.VARIANT_ID}.  This probably means the product hasn't been loaded to shopify yet.");
                        return;
                    }
                    else
                    {
                        variantMap = variantMapDictionary[item.VARIANT_ID];
                    }


                    // Determine what kind of record we are dealing with (contract price, last purchase price or template price)
                    // and insert/update the record in the appropriate spot in the database.
                    Decimal price = 0;
                    string catalogName = "";
                    if (!string.IsNullOrWhiteSpace(item.CONTRACT_PRICE))
                    {
                        catalogName = $"Contract_{item.COMPANY_ID}";
                        contractCatalogs.TryAdd(catalogName, item.COMPANY_ID);
                        price = Decimal.Parse(item.CONTRACT_PRICE);
                        await UpsertContractPricing(brand, item, variantMap);

                    }
                    else if (!string.IsNullOrWhiteSpace(item.LAST_PURCHASE_PRICE))
                    {
                        catalogName = $"Company_{item.COMPANY_ID}";
                        lastPurchaseCatalogs.TryAdd(catalogName, item.COMPANY_ID);
                        price = Decimal.Parse(item.LAST_PURCHASE_PRICE);

                        if (companyIdMap.ContainsKey(item.COMPANY_ID))
                        {
                            await UpsertLastPurchasePricing(brand, item, variantMap, companyIdMap[item.COMPANY_ID]);
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(item.TEMPLATE_PRICE) && !string.IsNullOrWhiteSpace(item.TEMPLATE_NAME))
                    {
                        // WSSBPB-617 - We are no longer creating template catalogs on shopify. We are just loading the template pricing to the database and prices will be pulled via API
                        price = Decimal.Parse(item.TEMPLATE_PRICE);
                        await UpsertTemplatePricing(brand, item, variantMap, price);
                    }
                    else
                    {
                        _logger.LogWarning($"Could not determine record type for row {recordCount} as it has no contract or last purchase price or template price (or template price was specified but template name is missing.)");
                        return;
                    }

                    // round the price up to the nearest penny for Shopify.
                    price = Math.Ceiling(price * 100) / 100;

                    // Build a dictionary with the pricing for each catalog that we'll use later to update the pricing
                    // in shopify.
                    catalogToVariantPriceMap.TryAdd(catalogName, new ConcurrentBag<Tuple<string, decimal>>());
                    catalogToVariantPriceMap[catalogName].Add(new Tuple<string, decimal>(variantMap.ShopifyVariantID, price));
                    catalogToProductMap.TryAdd(catalogName, new ConcurrentBag<string>());
                    catalogToProductMap[catalogName].Add(variantMap.ShopifyProductID);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception occurred while processing record #{recordCount}");
                    Interlocked.Increment(ref exceptionCount);
                }
            });

            _logger.LogInformation("Finished processing CSV Records.");

            // Go through the contract catalogs.
            await contractCatalogs.ParallelForEachAsync(async entry =>
            {
                var catalogName = entry.Key;
                var contractId = entry.Value;

                try
                {
                    // Create the catalog if necessary.  If we do create it, then associate the
                    // necessary companies with the catalog.
                    _logger.LogInformation($"Processing contract catalog {catalogName}");
                    (var catalog, bool isNew) = await GetOrCreateShopifyCatalog(catalogName);
                    if (contractToCompanyListMap.ContainsKey(contractId) && isNew) // TODO: Add && isNew?
                    {
                        var companiesWithCatalog = contractToCompanyListMap[contractId];
                        if (!companiesWithCatalog.Any())
                        {
                            _logger.LogWarning($"No companies found with contract id {contractId}");
                        }
                        bool success = await EnsureCompaniesHaveCatalog(catalog.Id, companiesWithCatalog, companyIdMap);

                        // Increment exception count if there were any errors.  No need to log anything as the error
                        // should already be logged by the EnsureCompaniesHaveCatalog method.
                        if (!success) Interlocked.Increment(ref exceptionCount);
                    }

                    // Add the prices to the catalog.
                    await _priceListService.UpsertVariantsToPriceList(catalog.PriceListId, "USD", catalogToVariantPriceMap[catalogName].Select(t => Tuple.Create($"gid://shopify/ProductVariant/{t.Item1}", t.Item2)));

                    // Add the products to the publication.
                    await _catalogService.UpsertProductsToCatalogPublication(catalog.PublicationId,
                        catalogToProductMap[catalogName]
                        .Distinct()
                        .Select(x => $"gid://shopify/Product/{x}"));

                    // TODO: Need to check if any companies that are associated to this contract have any of these
                    // items in their "Company_XXXXX" last-purchase pricing catalog and remove those entries if they
                    // exist.
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception occurred while processing contract catalog {catalogName}.");
                    Interlocked.Increment(ref exceptionCount);
                }
            });

            await lastPurchaseCatalogs.ParallelForEachAsync(async entry =>
            {
                var catalogName = entry.Key;
                var externalCompanyId = entry.Value;

                try
                {
                    // Create the catalog if necessary.  If we do create it, then associate the
                    // necessary companies with the catalog.
                    _logger.LogInformation($"Processing company catalog {catalogName}");
                    (var catalog, bool isNew) = await GetOrCreateShopifyCatalog(catalogName);
                    if (companyIdMap.ContainsKey(externalCompanyId) && isNew)
                    {
                        var companiesWithCatalog = new List<string>() { externalCompanyId };
                        bool success = await EnsureCompaniesHaveCatalog(catalog.Id, companiesWithCatalog, companyIdMap);

                        // Increment exception count if there were any errors.  No need to log anything as the error
                        // should already be logged by the EnsureCompaniesHaveCatalog method.
                        if (!success) Interlocked.Increment(ref exceptionCount);
                    }
                    else if (!companyIdMap.ContainsKey(externalCompanyId))
                    {
                        _logger.LogWarning($"Found last purchase price for company {externalCompanyId} but that company isn't in Shopify");
                    }

                    // Check if any of the items we are adding to this last purchase catalog already exist
                    // in the company's contract catalog (if they have one) and DO NOT add those items to this catalog.
                    // We need to do this because if the item is in both the contract catalog and the last purchase
                    // catalog, then Shopify will pick the lowest price from the two catalogs, which is not what Winzer
                    // wants - they want Contract pricing to always override last purchase pricing.
                    var variantsToAdd = catalogToVariantPriceMap[catalogName].Select(t => Tuple.Create($"gid://shopify/ProductVariant/{t.Item1}", t.Item2));
                    var contractsForCompany = contractToCompanyListMap.Where(e => e.Value.Contains(externalCompanyId)).ToList(); // Dictionary abuse..
                    var contractId = contractsForCompany.Any() ? contractsForCompany.First().Key : null;
                    if (contractId != null)
                    {
                        (variantsToAdd, bool success) = await PruneItemsThatAreInContractPriceList(externalCompanyId, contractId, variantsToAdd);
                        if (!success) Interlocked.Increment(ref exceptionCount);
                    }
                    // Add the prices to the catalog.
                    await _priceListService.UpsertVariantsToPriceList(catalog.PriceListId, "USD", variantsToAdd);

                    // Add the products to the publication (I think this is safe, even if it's not filtered down.)
                    await _catalogService.UpsertProductsToCatalogPublication(catalog.PublicationId,
                        catalogToProductMap[catalogName]
                        .Distinct()
                        .Select(x => $"gid://shopify/Product/{x}"));

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception occurred while processing contract catalog {catalogName}.");
                    Interlocked.Increment(ref exceptionCount);
                }
            });

            return exceptionCount == 0;
        }

        private async Task<(List<Tuple<string, decimal>>, bool)> PruneItemsThatAreInContractPriceList(string externalCompanyId, string contractId, IEnumerable<Tuple<string, decimal>> variantPrices)
        {
            var rval = new List<Tuple<string, decimal>>();
            bool success = true;

            try
            {
                HashSet<string> contractPriceListItems = new HashSet<string>();
                var contractCatalog = await _catalogService.GetCatalogByName($"Contract_{contractId}");
                if (contractCatalog != null && !string.IsNullOrWhiteSpace(contractCatalog.PriceListId))
                {
                    var contractItemList = await _priceListService.GetPriceListItems(contractCatalog.PriceListId);
                    foreach (var item in contractItemList) contractPriceListItems.Add(item.VariantId);
                }

                foreach (var variantPrice in variantPrices)
                {
                    if (!contractPriceListItems.Contains(variantPrice.Item1))
                    {
                        rval.Add(variantPrice);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while attempting to prune last purchase prices for company {externalCompanyId} with contract {contractId}");
                success = false;
            }

            return (rval, success);
        }

        private async Task<bool> EnsureCompaniesHaveCatalog(string catalogId, List<string> companiesWithCatalog, IDictionary<string, string> companyIdMap)
        {
            bool success = true;
            foreach (var externalId in companiesWithCatalog)
            {
                try
                {
                    if (companyIdMap.ContainsKey(externalId))
                    {
                        var locations = await _companyService.GetLocationsForCompany(companyIdMap[externalId]);
                        var addResult = await _catalogService.AddCatalogToCompanyLocations(catalogId, locations.Select(l => l.Id));
                        if (!addResult) success = false;
                    }
                    else
                    {
                        _logger.LogWarning($"Company with external id {externalId} was not found in Shopify.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception while associating catalog {catalogId} to company locations for company {externalId}.  This will need to be corrected manually.");
                    success = false;
                }
            }

            return success;
        }

        private async Task<(ShopifyCompanyLocationCatalog, bool)> GetOrCreateShopifyCatalog(string catalogName)
        {
            bool isNew = false;
            // Get (or create) the new catalog in Shopify.
            var catalog = await _catalogService.GetCatalogByName(catalogName);
            if (catalog == null)
            {

                isNew = true;
                catalog = new()
                {
                    Status = "ACTIVE",
                    Title = catalogName,
                };
                _logger.LogInformation($"Creating catalog {catalog.Title}");
                catalog.Id = await _catalogService.CreateCatalog(Enumerable.Empty<string>(), catalog);
            }

            // Create the price list for the catalog in Shopify if necessary.
            if (string.IsNullOrWhiteSpace(catalog.PriceListId))
            {
                //create the pricelist
                var priceList = new ShopifyPriceList
                {
                    Name = catalog.Title + "_pricelist",
                    CatalogId = catalog.Id,
                    CurrencyCode = "USD"
                };

                _logger.LogInformation($"Creating price list {priceList.Name}");
                catalog.PriceListId = await _priceListService.CreatePriceList(priceList);
            }

            // Create the publication if necessary.
            if (string.IsNullOrWhiteSpace(catalog.PublicationId))
            {
                _logger.LogInformation($"Creating publication for catalog {catalog.Id}");
                catalog.PublicationId = await _catalogService.CreatePublicationForCatalog(catalog.Id, false, false);
            }

            return (catalog, isNew);
        }

        private async Task PopulateCompanyMappings(IDictionary<string, List<string>> contractToCompanyListMap, IDictionary<string, List<string>> templateToCompanyListMap, IDictionary<string, string> companyIdMap)
        {
            _logger.LogInformation("Loading all companies from Shopify...");
            var companies = await _companyService.GetAllCompanies();
            _logger.LogInformation("Finished loading companies.");

            if (companies.Companies != null && companies.Companies.Any())
            {
                foreach (var company in companies.Companies)
                {
                    if (string.IsNullOrWhiteSpace(company.ExternalId)) continue;

                    // Add the company to our winzer id -> shopify id mapping dictionary.
                    companyIdMap.TryAdd(company.ExternalId, RemoveGIDCrapFromShopifyId(company.Id));

                    // If the company has a pricing contract, add them to the list of companies with that contract.
                    if (!string.IsNullOrWhiteSpace(company.PricingContract))
                    {
                        List<string> val;
                        if (contractToCompanyListMap.TryGetValue(company.PricingContract, out val))
                        {
                            val.Add(company.ExternalId);
                        }
                        else
                        {
                            val = new List<string>()
                                {
                                    company.ExternalId
                                };
                            contractToCompanyListMap.Add(company.PricingContract, val);
                        }
                    }

                    // If the company has a template, add them to the list of companies with that tempate.
                    if (!string.IsNullOrWhiteSpace(company.TemplateName))
                    {
                        List<string> val;
                        if (templateToCompanyListMap.TryGetValue(company.TemplateName, out val))
                        {
                            val.Add(company.ExternalId);
                        }
                        else
                        {
                            val = new List<string>() { company.ExternalId };
                            templateToCompanyListMap.Add(company.TemplateName, val);
                        }
                    }
                }
            }
        }

        private BrandEnum? GetDatabaseBrand(string companyName)
        {
            if (companyName == ONESOURCE)
            {
                return BrandEnum.OneSource;
            }
            else if (companyName == WINZER)
            {
                return BrandEnum.Winzer;
            }
            else if (companyName == FASTSERV)
            {
                return BrandEnum.FastServ;
            }

            return null;
        }

        private async Task UpsertContractPricing(BrandEnum brand, PricingCSVRecord item, VariantMap variantMap)
        {
            var pricingItem = new ContractPricing
            {
                BrandID = brand,
                ContractID = item.COMPANY_ID,
                ShopifyVariantID = variantMap.ShopifyVariantID,
                ShopifyProductID = variantMap.ShopifyProductID,
            };

            if (!string.IsNullOrWhiteSpace(item.CONTRACT_PRICE))
            {
                pricingItem.ContractPrice = Decimal.Parse(item.CONTRACT_PRICE);
            }

            await _contractPricingService.MergeContractPricing(pricingItem);
        }

        private async Task UpsertLastPurchasePricing(BrandEnum brand, PricingCSVRecord item, VariantMap variantMap, string shopifyCompanyId)
        {

            var pricingItem = new LastPurchasePricing
            {
                BrandID = brand,
                ShopifyCompanyID = shopifyCompanyId,
                ShopifyVariantID = variantMap.ShopifyVariantID,
                ShopifyProductID = variantMap.ShopifyProductID,
            };

            if (!string.IsNullOrWhiteSpace(item.LAST_PURCHASE_PRICE))
            {
                pricingItem.LastPurchasePrice = Decimal.Parse(item.LAST_PURCHASE_PRICE);
            }

            await _lastPurchasePricingService.MergeLastPurchasePricing(pricingItem);
        }

        private async Task UpsertTemplatePricing(BrandEnum brand, PricingCSVRecord item, VariantMap variantMap, decimal price)
        {
            var pricingItem = new TemplatePricing
            {
                BrandID = brand,
                TemplateName = item.TEMPLATE_NAME,
                ShopifyVariantID = variantMap.ShopifyVariantID,
                ShopifyProductID = variantMap.ShopifyProductID,
            };

            if (!string.IsNullOrWhiteSpace(item.TEMPLATE_PRICE))
            {
                pricingItem.TemplatePrice = Decimal.Parse(item.TEMPLATE_PRICE);
            }

            await _templatePricingService.MergeTemplatePricing(pricingItem);
        }

        private void UpsertBulkPricing(PricingCSVRecord item, VariantMap variantMap, decimal price)
        {
            int? quantity = null;
            var isNew = false;
            var isChanged = false;
            if (!string.IsNullOrWhiteSpace(item.QUANTITY))
            {
                quantity = int.Parse(item.QUANTITY);
            }
            else
            {
                quantity = 0;
            }

            var pricingItem = _bulkPricingService.GetBulkPricing(RemoveGIDCrapFromShopifyId(variantMap.ShopifyVariantID), quantity);
            if (pricingItem == null)
            {
                pricingItem = new BulkPricing
                {
                    ShopifyVariantID = RemoveGIDCrapFromShopifyId(variantMap.ShopifyVariantID),
                    ShopifyProductID = RemoveGIDCrapFromShopifyId(variantMap.ShopifyProductID),
                    quantity = quantity
                };
                isNew = true;
            }
            if (pricingItem.BulkPrice != price)
            {
                pricingItem.BulkPrice = price;
                isChanged = true;
            }
            if (isNew)
            {
                _bulkPricingService.CreateBulkPricing(pricingItem);
            }
            else if (isChanged)
            {
                _bulkPricingService.UpdateBulkPricing(pricingItem);
            }
        }

        private async Task<IList<PricingCSVRecord>> GetAllPricesFromCSV(string companyName, string filePath)
        {
            // todo: switch this to a dynamic file after we start downloading the file from ftp.
            //string filePath = "C:\\Repositories\\Winzer\\Resources\\SamplePricingCSV.csv";

            //if (companyName == FASTSERV)
            //{
            //    filePath = "C:\\Users\\eric.petroelje\\Downloads\\FastServ_Pricing_V2_092224.csv";
            //}
            //if (companyName == WINZER)
            //{
            //    filePath = "C:\\Users\\eric.petroelje\\Downloads\\Franchise_Pricing_090624_V1.csv";
            //}
            //if (companyName == ONESOURCE)
            //{
            //    filePath = "C:\\Repositories\\Winzer\\Resources\\SamplePricingCSVOneSource.csv";
            //}

            var csvBytes = await _sftpService.DownloadFileAsync(filePath);

            using (var reader = new StreamReader(new MemoryStream(csvBytes)))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<PricingCSVRecord>().ToList();

                return records;
            }
        }

        private static string RemoveGIDCrapFromShopifyId(string idWithGidCrap)
        {
            // Takes a string like "gid://shopify/ProductVariant/1341341343" and returns just "1341341343"
            return idWithGidCrap[(idWithGidCrap.LastIndexOf('/') + 1)..];
        }
    }
}

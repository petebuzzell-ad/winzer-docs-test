using Winzer.Library.Salsify;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Winzer.Impl.Salsify
{
    public class SalsifyAPIServiceOptions
    {
        public string APIKey { get; set; }
        public string OrganizationId { get; set; }
        public string[] OptionFieldNames { get; set; }
        public Dictionary<string, List<string>> SizeSortDictionary { get; set; }
    }

    public class SalsifyAPIService : ISalsifyAPIService
    {
        private readonly string _apiKey;
        private readonly string _organizationId;
        private readonly ILogger _logger;

        public SalsifyAPIService(SalsifyAPIServiceOptions options, ILogger<SalsifyAPIService> logger)
        {
            _apiKey = options.APIKey;
            _organizationId = options.OrganizationId;
            _logger = logger;
        }

        public async Task UpdateProductsBatch(IEnumerable<SalsifyProduct> products)
        {
            var updateData = new List<Dictionary<string, object?>>();

            foreach ( var product in products )
            {
                var entry = new Dictionary<string, object?>();
                entry["salsify:id"] = product.SalsifyId;
                foreach ( var prop in product.Properties )
                {
                    if (!prop.Key.StartsWith("salsify"))
                    {
                        entry[prop.Key] = prop.Value;
                    }
                }

                updateData.Add(entry);
            }

            var json = JsonConvert.SerializeObject(updateData);

            using var client = new HttpClient();
            var url = $"https://app.salsify.com/api/v1/orgs/{_organizationId}/products";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var body = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, body);
            if (!response.IsSuccessStatusCode)
            {
                string message = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                throw new Exception($"Call to Salsify Batch Update API failed with code {response.StatusCode}: {message}");
            }
        }

        public async Task<SalsifyProduct?> GetProductById(string productId)
        {
            using var client = new HttpClient();
            var url = $"https://app.salsify.com/api/v1/orgs/{_organizationId}/products/{productId}";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            else if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get product {productId} from Salsify.  Status: {response.StatusCode} Response Was: {message}");
            }

            var json = await response.Content.ReadAsStringAsync();

            return ParseSalsifyResponse(json);
        }

        private SalsifyProduct ParseSalsifyResponse(string json)
        {
            var rval = new SalsifyProduct();

            var jobj = JObject.Parse(json);
            foreach ( var elem in jobj )
            {
                if (elem.Key == "salsify:id")
                {
                    rval.SalsifyId = elem.Value?.ToString();
                }
                else if(elem.Key == "salsify:parent_id")
                {
                    rval.ParentId = elem.Value?.ToString();
                }
                else if (elem.Key.StartsWith("salsify"))
                {
                    continue; // Ignore other "salsify" properties.
                }
                else if(elem.Key == "Shopify Product ID")
                {
                    rval.ShopifyId = elem.Value?.ToString();
                }
                else if (elem.Value?.Type == JTokenType.String)
                {
                    rval.Properties.Add(elem.Key, elem.Value.Value<string>());
                }
                else if (elem.Value?.Type == JTokenType.Boolean)
                {
                    rval.Properties.Add(elem.Key, elem.Value.Value<bool>());
                }
                else if (elem.Value?.Type == JTokenType.Integer)
                {
                    rval.Properties.Add(elem.Key, elem.Value.Value<int>());
                }
                else if (elem.Value?.Type == JTokenType.Float)
                {
                    rval.Properties.Add(elem.Key, elem.Value.Value<decimal?>());
                }
            }

            return rval;
        }

        public async Task UpdateProductProperty(string productId, string propertyName, string propertyValue)
        {
            using var client = new HttpClient();
            var url = $"https://app.salsify.com/api/v1/orgs/{_organizationId}/products";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var json =
$@"
[
   {{
    ""ProductID"": ""{productId}"",
    ""{propertyName}"": ""{propertyValue}""
   }}
]
";
            var body = new StringContent(json, Encoding.UTF8, "application/json");
            _logger.LogDebug(json);
            var response = await client.PutAsync(url, body);
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update {propertyName} to {propertyValue} on product {productId}.  Response was: {message}");
            }
        }
        public async Task CreateProductImport(string importID)
        {
            // Start the import
            var importResponse = await StartImport(importID);
            if (!string.IsNullOrWhiteSpace(importResponse.failure_reason))
            {
                throw new Exception($"Import failed with reason: {importResponse.failure_reason}");
            }

            if (string.IsNullOrWhiteSpace(importResponse.id))
            {
                throw new Exception($"Import response did not have a run id on it, but had no failure reason!");
            }

            // Wait for it to finish (this could take a while)
            var importStatusResponse = await WaitForImportToComplete(importResponse.id);

            if(importStatusResponse != "completed")
            {
                throw new Exception($"Failed to complete import with this response: {importStatusResponse}");
            }
        }
        public async Task<string> WaitForImportToComplete(string importId)
        {
            using (var client = new HttpClient())
            {
                var url = $"https://app.salsify.com/api/orgs/{_organizationId}/imports/runs/{importId}";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                while (true)
                {
                    var response = await client.GetAsync(url);
                    if (response != null && response.Content != null)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        _logger.LogDebug(json);

                        var responseData = JsonConvert.DeserializeObject<SalsifyImportStatusResponse>(json);
                        if (responseData == null)
                        {
                            throw new Exception($"Could not parse response data while waiting for import to complete.  Response status code was {response.StatusCode}");
                        }

                        if (!string.IsNullOrEmpty(responseData.failure_reason))
                        {
                            throw new Exception($"Import failed with reason: {responseData.failure_reason}");
                        }

                        if (responseData.status == "running" || responseData.status == "completing")
                        {
                            continue;
                        }
                        if (responseData.status == "completed")
                        {
                            return responseData.status;
                        }
                    }
                    else
                    {
                        throw new Exception($"Request to get import status failed with code: {response?.StatusCode}");
                    }

                    // Wait for a bit, then try again.
                    await Task.Delay(3000);
                }
            }
        }
        private async Task<SalsifyStartImportResponse> StartImport(string importID)
        {
            using (var client = new HttpClient())
            {
                var url = $"https://app.salsify.com/api/orgs/{_organizationId}/imports/{importID}/runs";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                var serializationOptions = new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var content = JsonContent.Create(url, MediaTypeHeaderValue.Parse("application/json"), serializationOptions);

                var response = await client.PostAsync(url, content);
                if (response != null && response.Content != null)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug(responseText);

                    var rval = JsonConvert.DeserializeObject<SalsifyStartImportResponse>(responseText);
                    if (rval != null)
                    {
                        return rval;
                    }
                }

                throw new Exception($"Got unsuccessful response while creating import: {response?.StatusCode}");
            }
        }
        public async Task<Stream> CreateProductExport(SalsifyExportRequest req)
        {
            // Start the export
            var exportResponse = await StartExport(req);
            if (!string.IsNullOrWhiteSpace(exportResponse.failure_reason))
            {
                throw new Exception($"Export failed with reason: {exportResponse.failure_reason}");
            }

            if (string.IsNullOrWhiteSpace(exportResponse.id))
            {
                throw new Exception($"Export response did not have a run id on it, but had no failure reason!");
            }
            // A batch of 100 products takes ~6 seconds.  Wait first because polling counts against our API limit.
            await Task.Delay(6000);
            // Wait for it to finish (this could take a while)
            var exportUrl = await WaitForExportToComplete(exportResponse.id);

            // Stream back the resulting json/csv/whatever.
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(exportUrl);
                if (response != null && response.Content != null && response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStreamAsync();
                }
                else
                {
                    throw new Exception($"Failed to get export file from export URL.  Response status code was {response?.StatusCode}");
                }
            }
        }

        private async Task<string> WaitForExportToComplete(string exportId) {
            using var client = new HttpClient();
            var url = $"https://app.salsify.com/api/orgs/{_organizationId}/export_runs/{exportId}";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            while (true)
            {
                var response = await client.GetAsync(url);
                if (response != null && response.Content != null)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug(json);

                    var responseData = JsonConvert.DeserializeObject<SalsifyExportStatusResponse>(json);
                    if (responseData == null)
                    {
                        throw new Exception($"Could not parse response data while waiting for export to complete.  Response status code was {response.StatusCode}");
                    }

                    if (!string.IsNullOrEmpty(responseData.failure_reason))
                    {
                        throw new Exception($"Export failed with reason: {responseData.failure_reason}");
                    }

                    if (!string.IsNullOrWhiteSpace(responseData.url) && responseData.status == "completed")
                    {
                        return responseData.url;
                    }

                    if (responseData.status != "running")
                    {
                        throw new Exception($"Got unexpected export status of {responseData.status} assuming this means a failure.");
                    }
                }
                else
                {
                    throw new Exception($"Request to get export status failed with code: {response?.StatusCode}");
                }

                // Wait for a bit, then try again.
                await Task.Delay(3000);
            }
        }

        private async Task<SalsifyStartExportResponse> StartExport(SalsifyExportRequest req)
        {
            using var client = new HttpClient();
            var url = $"https://app.salsify.com/api/orgs/{_organizationId}/export_runs";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var requestData = new
            {
                configuration = new
                {
                    entity_type = "product",
                    include_all_columns = req.IncludeAllColumns,
                    format = req.Format,
                    filter = req.Filter,
                    properties = req.PropertiesToExport.Any() ? "'" + string.Join("','", req.PropertiesToExport) + "'" : null
                }
            };

            var serializationOptions = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var content = JsonContent.Create(requestData,  MediaTypeHeaderValue.Parse("application/json"), serializationOptions);
            _logger.LogDebug(await content.ReadAsStringAsync());

            var response = await client.PostAsync(url, content);
            if (response != null && response.Content != null && response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                _logger.LogDebug(responseText);

                var rval = JsonConvert.DeserializeObject<SalsifyStartExportResponse>(responseText);
                if (rval != null)
                {
                    return rval;
                }
            }

            throw new Exception($"Got unsuccessful response while creating export: {response?.StatusCode}");
        }
    }

    #pragma warning disable IDE1006 // Naming Styles
    class SalsifyStartExportResponse
    {
        public string id { get; set; } = string.Empty;

        public string status { get; set; } = string.Empty;

        public DateTime? start_time { get; set; }

        public DateTime? end_time { get; set; }

        public decimal? duration { get; set; }

        public string? url { get; set; }

        public decimal? progress { get; set; }

        public string? failure_reason { get; set; }

        public string? estimated_time_remaining { get; set; }
    }

    class SalsifyExportStatusResponse
    {
        public string? id { get; set; }

        public string? status { get; set; }

        public DateTime? start_time { get; set; }

        public DateTime? end_time { get; set; }

        public decimal? duration { get; set; }

        public string? url { get; set; }

        public decimal? progress { get; set; }

        public DateTime? includes_changes_before { get; set; }

        public string? failure_reason { get; set; }

        public string? estimated_time_remaining { get; set; }
    }

    class SalsifyImportStatusResponse
    {
        public string? id { get; set; }

        public string? status { get; set; }
        public string? status_summary { get; set; }

        public DateTime? start_time { get; set; }

        public DateTime? end_time { get; set; }

        public decimal? duration { get; set; }

        public string? url { get; set; }

        public decimal? progress { get; set; }

        public DateTime? includes_changes_before { get; set; }

        public string? failure_reason { get; set; }

        public string? estimated_time_remaining { get; set; }
    }

    class SalsifyStartImportResponse
    {
        public string id { get; set; } = string.Empty;

        public string status { get; set; } = string.Empty;
        public string? status_summary { get; set; }

        public DateTime? start_time { get; set; }

        public DateTime? end_time { get; set; }

        public decimal? duration { get; set; }

        public decimal? progress { get; set; }

        public string? failure_reason { get; set; }

        public string? estimated_time_remaining { get; set; }
    }
    #pragma warning restore IDE1006 // Naming Styles
}

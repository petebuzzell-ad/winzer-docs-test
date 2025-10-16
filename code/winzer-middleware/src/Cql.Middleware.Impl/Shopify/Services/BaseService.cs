using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CQL.Middleware.Impl.Shopify.GraphQL;
using Cql.Middleware.Library.Shopify;
using System.Diagnostics;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using Cql.Middleware.Library.Shopify.Common;
using Dasync.Collections;
using System.Collections.Concurrent;
using GraphQL.Client.Serializer.Newtonsoft;

namespace Cql.Middleware.Impl.Shopify.Services
{
    public abstract class BaseService
    {
        protected readonly ILogger _logger;
        protected readonly ShopifyGraphQLOptions _options;

        protected BaseService(ILogger logger, ShopifyGraphQLOptions options)
        {
            _logger = logger;
            _options = options;
        }

        public async Task<IEnumerable<ShopifyMetaField>> SetMetafields(IEnumerable<ShopifyMetafieldsSetInput> metafieldsInput, CancellationToken cancellationToken = default)
    {
        var metafieldsSetQuery = @"
mutation metafieldsSet($metafields: [MetafieldsSetInput!]!) {
  metafieldsSet(metafields: $metafields) {
    metafields {
      ...metafieldFragment
    }
    userErrors {
      field
      message
    }
  }
}

fragment metafieldFragment on Metafield {
    id
    key
    value
    namespace
}";
        var metafields = new ConcurrentBag<ShopifyMetaField>();
        await metafieldsInput.Chunk(25).ParallelForEachAsync(async metafieldChunk => {
            var result = await ExecuteGraphQLQuery<ShopifyGraphQLMetafieldsSetPayload>(metafieldsSetQuery, new
            {
                metafields = metafieldChunk.Select(m => new ShopifyGraphQLMetafieldsSetInput
                {
                    ownerId = m.OwnerId,
                    @namespace = m.Namespace,
                    key = m.Key,
                    value = m.Value,
                    type = m.Type
                })
            }, cancellationToken: cancellationToken);

            if (result.userErrors.Any())
            {
                foreach (var userError in result.userErrors)
                {
                    _logger.LogError("Error on metafieldsSet: {0} - {1}", userError.field?.FirstOrDefault(), userError.message);
                }
            }
            foreach (var metafield in result.metafields)
            {
                metafields.Add(metafield.Map());
            }
        });

        return metafields.ToList();
    }


        protected virtual async Task<T> ExecuteGraphQLQuery<T>(string query, object parameters, bool retryOnFailure = true, CancellationToken cancellationToken = default)
        {
            var rando = new Random();
            int attempts = 0;
            while (true)
            {
                try
                {
                    return await ExecuteGraphQLQuery_Internal<T>(query, parameters, cancellationToken);
                }
                catch (Exception ex)
                {
                    if(attempts++ >= _options.RetryAttempts || !retryOnFailure)
                    {
                        _logger.LogDebug("Error with Query: {Query}", query);
                        throw;
                    }
                    _logger.LogDebug(ex, "On attempt {Attempts} query execution failed with exception: {Message}", attempts, ex.Message);
                    await Task.Delay(rando.Next(500 + 1000 * Math.Min(attempts, 4), 5000), cancellationToken);
                }
            }
        }

        private async Task<T> ExecuteGraphQLQuery_Internal<T>(string query, object parameters, CancellationToken cancellationToken, bool ignoreNullValues = true)
        {
            var serializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings { NullValueHandling = ignoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include });
            var client = new GraphQLHttpClient(_options.EndPointUrl(), serializer);
            client.HttpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", _options.AccessTokenValue);
            client.HttpClient.Timeout = TimeSpan.FromMinutes(10);

            var request = new GraphQLHttpRequest(query, parameters);
            var watch = Stopwatch.StartNew();
            GraphQLResponse<T> response;
            try
            {
                response = await client.SendQueryAsync<T>(request, cancellationToken);
            }
            finally
            {
                watch.Stop();
                _logger.LogDebug("Query: ({0}s) {1}", (watch.ElapsedMilliseconds / 1000.0), JsonConvert.SerializeObject(request));
            }

            if (response.HasErrors())
            {
                _logger.LogDebug("Error with Query: {0}", query);
                _logger.LogDebug(JsonConvert.SerializeObject(response));
                throw new GraphQLException(response.Errors!);
            }
            LogThrottleStatus(response.Extensions);

            return response.Data;
        }

        private void LogThrottleStatus(Map? extensions){
            var cost = extensions.Dig("cost");
            var throttleStatus = cost.Dig("throttleStatus");
            if (throttleStatus != null)
            {
                _logger.LogDebug("Executed query with cost: {0}.  Remaining quota: {1}", cost!.GetValueOrDefault("actualQueryCost"), throttleStatus.GetValueOrDefault("currentlyAvailable"));
            }
        }
    }
}

using Cql.Middleware.Library.Shopify;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Winzer.Impl.Setup
{
    public static class GraphQLSetup
    {
        public static void ConfigureGraphQL(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection("ShopifyGraphQL").Get<ShopifyGraphQLOptions>();
            services.AddSingleton<ShopifyGraphQLOptions>(options);
            //services.AddSingleton<IGraphQLClient, GraphQLHttpClient>();
            //services.AddSingleton<IGraphQLWebsocketJsonSerializer, NewtonsoftJsonSerializer>();
            //services.AddSingleton(new GraphQLHttpClientOptions());

            //services.AddHttpClient<IGraphQLClient, GraphQLHttpClient>(client =>
            //{
            //    client.BaseAddress = new Uri(options.EndPointUrl());
            //    client.Timeout = TimeSpan.FromMinutes(1);
            //    client.DefaultRequestHeaders.Add(options.AccessTokenName, options.AccessTokenValue);
            //});
        }
    }
}

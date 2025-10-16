using GraphQL;
using GraphQL.Client.Http;
using System.Net;

namespace CQL.Middleware.Impl.Shopify.GraphQL;

/// <summary>
/// An exception thrown on unexpected GraphQLError Response
/// </summary>
public class GraphQLException : Exception
{
    /// <summary>
    /// The returned Errors
    /// </summary>
    public GraphQLError[] Errors { get; }

    /// <summary>
    /// The first Error
    /// </summary>
    public GraphQLError Error { get => Errors.First(); }

    /// <summary>
    /// Creates a new instance of <see cref="GraphQLException"/>
    /// </summary>
    /// <param name="errors"></param>
    public GraphQLException(GraphQLError[] errors)
        : base(errors.First().Message)
    {
        Errors = errors;
    }

    /// <summary>
    /// Whether or not the GraphQLError code is a retryable case.
    /// Currently only Throttled is retryable.
    /// </summary>
    public bool Retryable()
    {
        return Error.IsThrottled() || Error.IsTimeout();
    }
}

public static class GraphQLHttpRequestExceptionExtensions
{
    private static readonly HttpStatusCode[] RetryableCodes = {
        HttpStatusCode.TooManyRequests,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.BadGateway
    };

    /// <summary>
    /// Whether or not the HTTP Request status code is a retryable case.
    /// </summary>
    public static bool Retryable(this GraphQLHttpRequestException ex)
    {
        return RetryableCodes.Contains(ex.StatusCode);
    }
}

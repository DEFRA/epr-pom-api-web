namespace WebApiGateway.Api.Extensions;

using System.Net.Http.Headers;

public static class HttpClientExtensions
{
    public static void AddIfNotExists(this HttpRequestHeaders headers, string key, string value)
    {
        if (!headers.Contains(key))
        {
            headers.Add(key, value);
        }
    }
}
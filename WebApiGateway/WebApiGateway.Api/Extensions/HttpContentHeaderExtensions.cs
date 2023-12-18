namespace WebApiGateway.Api.Extensions;

using System.Net.Http.Headers;

public static class HttpContentHeaderExtensions
{
    public static string? GetContentType(this HttpContentHeaders headers)
    {
        return headers.TryGetValues("Content-Type", out var matchingHeaders)
            ? matchingHeaders.FirstOrDefault().Split(";")[0]
            : default;
    }
}
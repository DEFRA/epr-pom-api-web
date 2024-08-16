namespace WebApiGateway.Api.Extensions;

using System.Net.Http.Headers;

public static class HttpContentHeaderExtensions
{
    public static string? GetContentType(this HttpContentHeaders headers)
    {
        var returnString = string.Empty;
        headers.TryGetValues("Content-Type", out var matchingHeaders);

        if (matchingHeaders != null)
        {
            var firstString = matchingHeaders.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(firstString))
            {
                returnString = firstString.Split(";")[0];
            }
        }
        return returnString;
    }
}
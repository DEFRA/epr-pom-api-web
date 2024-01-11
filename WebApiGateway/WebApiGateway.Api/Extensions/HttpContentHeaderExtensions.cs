namespace WebApiGateway.Api.Extensions;

using System.Net.Http.Headers;
using Microsoft.IdentityModel.Tokens;

public static class HttpContentHeaderExtensions
{
    public static string? GetContentType(this HttpContentHeaders headers)
    {
        var returnString = string.Empty;
        headers.TryGetValues("Content-Type", out var matchingHeaders);

        if (matchingHeaders != null)
        {
            var firstString = matchingHeaders.FirstOrDefault();

            if (firstString != null && !firstString.IsNullOrEmpty())
            {
                returnString = firstString.Split(";")[0];
            }
        }
        return returnString;
    }
}
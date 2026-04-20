using System.Net.Http.Headers;

namespace WebApiGateway.Api.Handlers;

public class OAuth2Handler(OAuth2TokenCache tokenCache) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await tokenCache.GetToken(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
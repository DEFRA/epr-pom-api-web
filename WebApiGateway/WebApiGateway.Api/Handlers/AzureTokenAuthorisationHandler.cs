using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;

namespace WebApiGateway.Api.Handlers;

[ExcludeFromCodeCoverage]
public abstract class AzureTokenAuthorisationHandler : DelegatingHandler
{
    private readonly TokenRequestContext _tokenRequestContext;
    private readonly DefaultAzureCredential? _credential;

    protected AzureTokenAuthorisationHandler(string? clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            return;
        }

        _tokenRequestContext = new TokenRequestContext(new[] { clientId });
        _credential = new DefaultAzureCredential();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_credential != null)
        {
            var tokenResult = await _credential.GetTokenAsync(_tokenRequestContext, cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue(Microsoft.Identity.Web.Constants.Bearer, tokenResult.Token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

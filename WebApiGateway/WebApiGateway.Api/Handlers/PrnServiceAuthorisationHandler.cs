using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using WebApiGateway.Core.Options;

namespace WebApiGateway.Api.Handlers
{
    [ExcludeFromCodeCoverage]
    public class PrnServiceAuthorisationHandler : DelegatingHandler
    {
        private readonly TokenRequestContext _tokenRequestContext;
        private readonly DefaultAzureCredential? _credentials;

        public PrnServiceAuthorisationHandler(IOptions<PrnServiceApiOptions> options)
        {
            if (!string.IsNullOrEmpty(options.Value.ClientId))
            {
                _tokenRequestContext = new TokenRequestContext(new[] { options.Value.ClientId });
                _credentials = new DefaultAzureCredential();
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await AddDefaultToken(request, cancellationToken);
            return await base.SendAsync(request, cancellationToken);
        }

        private async Task AddDefaultToken(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_credentials != null)
            {
                var tokenResult = await _credentials.GetTokenAsync(_tokenRequestContext, cancellationToken);
                request.Headers.Authorization = new AuthenticationHeaderValue(Constants.Bearer, tokenResult.Token);
            }
        }
    }
}

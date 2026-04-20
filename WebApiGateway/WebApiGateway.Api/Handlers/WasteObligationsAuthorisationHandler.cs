using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Extensions;

namespace WebApiGateway.Api.Handlers;

public class WasteObligationsAuthorisationHandler(IHttpContextAccessor httpContextAccessor, IAccountServiceClient accountServiceClient) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext.User.UserId();
        var organisationId = (await accountServiceClient.GetUserAccount(userId)).User.Organisations[0].Id;
        
        if (request.RequestUri.AbsolutePath.Contains("{organisationId}"))
        {
            var uriBuilder = new UriBuilder(request.RequestUri)
            {
                Path = request.RequestUri.AbsolutePath.Replace("{organisationId}", organisationId.ToString("D"))
            };
            request.RequestUri = uriBuilder.Uri;
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}
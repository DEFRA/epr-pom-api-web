using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Core.Constants;

namespace WebApiGateway.Api.Handlers;

public class WasteObligationsAuthorisationHandler(IHttpContextAccessor httpContextAccessor, IAccountServiceClient accountServiceClient) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var organisationId = GetComplianceSchemeId() ?? await GetOrganisationIdFromUserFallback();
        
        if (request.RequestUri.AbsolutePath.Contains(":organisationId"))
        {
            var uriBuilder = new UriBuilder(request.RequestUri)
            {
                Path = request.RequestUri.AbsolutePath.Replace(":organisationId", organisationId.ToString("D"))
            };
            
            request.RequestUri = uriBuilder.Uri;
        }
        
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<Guid> GetOrganisationIdFromUserFallback()
    {
        var userId = httpContextAccessor.HttpContext.User.UserId();
        var account = await accountServiceClient.GetUserAccount(userId);
        
        return account.User.Organisations[0].Id;
    }

    private Guid? GetComplianceSchemeId()
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null || !context.Items.TryGetValue(ComplianceScheme.ComplianceSchemeId, out var value))
        {
            return null;
        }

        if (value is Guid complianceSchemeId)
        {
            return complianceSchemeId;
        }

        if (value is string complianceSchemeIdString && Guid.TryParse(complianceSchemeIdString, out var parsedComplianceSchemeId))
        {
            return parsedComplianceSchemeId;
        }

        return null;
    }
}
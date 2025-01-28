using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Models.ComplianceSchemeDetails;

namespace WebApiGateway.Api.Services;

public class ComplianceSchemeDetailsService(
    IComplianceSchemeDetailsClient complianceSchemeDetailsClient,
    ILogger<ComplianceSchemeDetailsService> logger,
    IHttpContextAccessor httpContextAccessor)
    : IComplianceSchemeDetailsService
{
    public async Task<List<GetComplianceSchemeMemberDetailsResponse>?> GetComplianceSchemeDetails(int organisationId, Guid complianceSchemeId)
    {
        logger.LogDebug("Get Compliance Scheme Details For Organisation Id {OrganisationId}", organisationId);
        return await complianceSchemeDetailsClient.GetComplianceSchemeDetails(organisationId, complianceSchemeId);
    }

    public async Task<Guid?> GetComplianceSchemeIdAsync()
    {
        var context = httpContextAccessor.HttpContext;
        if (context != null && context.Items.TryGetValue(ComplianceScheme.ComplianceSchemeId, out var value))
        {
            // Simulate an asynchronous operation if needed
            await Task.CompletedTask;
            if (value is Guid complianceSchemeId)
            {
                return complianceSchemeId;
            }

            if (value is string complianceSchemeIdString && Guid.TryParse(complianceSchemeIdString, out var parsedComplianceSchemeId))
            {
                return parsedComplianceSchemeId;
            }
        }

        return null;
    }
}
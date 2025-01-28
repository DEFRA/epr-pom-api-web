using WebApiGateway.Core.Models.ComplianceSchemeDetails;

namespace WebApiGateway.Api.Services.Interfaces;

public interface IComplianceSchemeDetailsService
{
    Task<List<GetComplianceSchemeMemberDetailsResponse>?> GetComplianceSchemeDetails(int organisationId, Guid complianceSchemeId);

    Task<Guid?> GetComplianceSchemeIdAsync();
}
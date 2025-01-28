using WebApiGateway.Core.Models.ComplianceSchemeDetails;

namespace WebApiGateway.Api.Clients.Interfaces;

public interface IComplianceSchemeDetailsClient
{
    Task<List<GetComplianceSchemeMemberDetailsResponse>?> GetComplianceSchemeDetails(int organisationId, Guid complianceSchemeId);
}
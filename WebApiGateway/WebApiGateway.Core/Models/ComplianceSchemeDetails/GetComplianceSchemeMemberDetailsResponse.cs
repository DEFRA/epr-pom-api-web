using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.ComplianceSchemeDetails;

[ExcludeFromCodeCoverage]
public class GetComplianceSchemeMemberDetailsResponse
{
    public string MemberId { get; set; } = string.Empty;    // OrganisationNumber

    public string MemberType { get; set; } = string.Empty;

    public bool IsOnlineMarketplace { get; set; }

    public bool IsLateFeeApplicable { get; set; } = false;

    public int NumberOfSubsidiaries { get; set; }

    public int NumberOfSubsidiariesBeingOnlineMarketPlace { get; set; }
}
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Api.Clients;

public static class SubmissionStatusQueryBuilder
{
    public static string BuildSubmissionsEndpointQueryString(Guid organisationId, Guid? complianceSchemeId, int? year, SubmissionType submissionType, string? registrationJourney)
    {
        var endpoint = $"submissions/submissions?Type={submissionType}&OrganisationId={organisationId}";

        if (complianceSchemeId is not null && complianceSchemeId != Guid.Empty)
        {
            endpoint += $"&ComplianceSchemeId={complianceSchemeId}";
        }

        if (year is not null)
        {
            endpoint += $"&Year={year}";
        }

        if (registrationJourney is not null)
        {
            endpoint += $"&RegistrationJourney={registrationJourney}";
        }

        return endpoint;
    }
}
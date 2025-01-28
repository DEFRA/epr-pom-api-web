using System.Net;
using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Core.Models.ComplianceSchemeDetails;

namespace WebApiGateway.Api.Clients;

public class ComplianceSchemeDetailsClient(
    HttpClient commonDataApiHttpClient,
    ILogger<ComplianceSchemeDetailsClient> logger)
    : IComplianceSchemeDetailsClient
{
    public async Task<List<GetComplianceSchemeMemberDetailsResponse>?> GetComplianceSchemeDetails(int organisationId, Guid complianceSchemeId)
    {
        var response = await commonDataApiHttpClient.GetAsync($"cso-member-details/get-cso-member-details/{organisationId}/{complianceSchemeId}");

        // response.StatusCode == HttpStatusCode.BadRequest
        // response.StatusCode == HttpStatusCode.NoContent
        // response.StatusCode == HttpStatusCode.InternalServerError
        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError("Error Getting Compliance Scheme Details, Response status code does not indicate success: {StatusCode} ({ReasonPhrase})", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<GetComplianceSchemeMemberDetailsResponse>>(content);
    }
}
using System.Net;
using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Core.Models.Commondata;
using WebApiGateway.Core.Models.PackagingResubmissionApplication;

namespace WebApiGateway.Api.Clients;

public class CommondataClient(
    HttpClient httpClient,
    ILogger<ICommondataClient> logger)
    : ICommondataClient
{
    public async Task<SynapseResponse?> GetPackagingResubmissionFileDetailsFromSynapse(Guid fileId)
    {
        var response = await httpClient.GetAsync($"submissions/is_file_synced_with_cosmos/{fileId}");

        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError("Error Getting file status from synapse, StatusCode : {StatusCode} ({ReasonPhrase})", response.StatusCode, response.ReasonPhrase);
            return new SynapseResponse();
        }

        var content = await response.Content.ReadAsStringAsync();

        if (bool.TryParse(content, out bool result))
        {
            return new SynapseResponse() { IsFileSynced = result };
        }
        else
        {
            logger.LogError("Invalid response from common data endpoint {content}", content);
            return new SynapseResponse();
        }
    }

    public async Task<PackagingResubmissionMemberResponse?> GetPackagingResubmissionMemberDetails(Guid submissionId, string complianceSchemeId)
    {
        var content = string.Empty;
        try
        {
            var endpoint = $"submissions/pom-resubmission-paycal-parameters/{submissionId}?ComplianceSchemeId={complianceSchemeId}";

            var response = await httpClient.GetAsync(endpoint);
            content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<PackagingResubmissionMemberResponse>(content);
        }
        catch (Exception ex)
        {
            logger.LogError("Error Getting packaging resubmission member details for SubmissionId : {submissionId} and ComplianceSchemeId : {complianceSchemeId}", submissionId, complianceSchemeId);

            if (ex.GetType() == typeof(HttpRequestException))
            {
                var requestException = ex as HttpRequestException;
                if (requestException.StatusCode.HasValue && requestException.StatusCode.Value == HttpStatusCode.PreconditionRequired)
                {
                    return new PackagingResubmissionMemberResponse()
                    {
                        ErrorMessage = content
                    };
                }
            }

            throw;
        }
    }

    public async Task<PackagingResubmissionActualSubmissionPeriodResponse> GetActualSubmissionPeriod(Guid submissionId, string submissionPeriod)
    {
        var endpoint = $"submissions/get_actual_submission_period/{submissionId}/?SubmissionPeriod={submissionPeriod}";

        var response = await httpClient.GetAsync(endpoint);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError("Error Getting actual submission period for submission, StatusCode : {StatusCode} ({ReasonPhrase}), SubmissionId: {SubmissionId}, Submission Period: {submissionPeriod}", response.StatusCode, response.ReasonPhrase, submissionId, submissionPeriod);
            return new PackagingResubmissionActualSubmissionPeriodResponse { ActualSubmissionPeriod = submissionPeriod };
        }

        var content = await response.Content.ReadFromJsonAsync<PackagingResubmissionActualSubmissionPeriodResponse>();

        return content;
    }
}
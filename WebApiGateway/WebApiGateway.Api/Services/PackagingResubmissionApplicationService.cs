using EPR.SubmissionMicroservice.Application.Features.Queries.Common;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.PackagingResubmissionApplication;

namespace WebApiGateway.Api.Services;

public class PackagingResubmissionApplicationService(
    ISubmissionStatusClient submissionStatusClient,
    ICommondataClient commondataClient)
    : IPackagingResubmissionApplicationService
{
    public async Task<List<PackagingResubmissionApplicationDetails?>> GetPackagingResubmissionApplicationDetails(string request)
    {
        var applicationDetailsForMultipleSubmissionPeriods = await submissionStatusClient.GetPackagingResubmissionApplicationDetails(request);
        if (applicationDetailsForMultipleSubmissionPeriods == null)
        {
            return null;
        }

        foreach (var applicationDetail in applicationDetailsForMultipleSubmissionPeriods)
        {
            if (applicationDetail?.LastSubmittedFile?.FileId is not null && applicationDetail.IsSubmitted)
            {
                applicationDetail.SynapseResponse = await commondataClient.GetPackagingResubmissionFileDetailsFromSynapse(applicationDetail.LastSubmittedFile.FileId.Value);
            }
        }

        return applicationDetailsForMultipleSubmissionPeriods;
    }

    public async Task<PackagingResubmissionMemberResponse> GetPackagingResubmissionMemberDetails(Guid submissionId, string complianceSchemeId)
    {
        var result = await commondataClient.GetPackagingResubmissionMemberDetails(submissionId, complianceSchemeId);

        return result;
    }

    public async Task CreateEventAsync<T>(T @event, Guid submissionId)
        where T : AbstractEvent
    {
        await submissionStatusClient.CreateEventAsync<T>(@event, submissionId);
    }
}
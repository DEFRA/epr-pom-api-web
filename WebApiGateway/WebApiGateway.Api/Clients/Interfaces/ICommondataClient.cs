using WebApiGateway.Core.Models.Commondata;
using WebApiGateway.Core.Models.PackagingResubmissionApplication;

namespace WebApiGateway.Api.Clients.Interfaces;

public interface ICommondataClient
{
    Task<SynapseResponse?> GetPackagingResubmissionFileDetailsFromSynapse(Guid fileId);

    Task<PackagingResubmissionMemberResponse?> GetPackagingResubmissionMemberDetails(Guid submissionId, string complianceSchemeId);

    Task<PackagingResubmissionActualSubmissionPeriodResponse> GetActualSubmissionPeriod(Guid submissionId, string submissionPeriod);
}
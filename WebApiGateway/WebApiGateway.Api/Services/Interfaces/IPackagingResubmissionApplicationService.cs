using EPR.SubmissionMicroservice.Application.Features.Queries.Common;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.PackagingResubmissionApplication;

namespace WebApiGateway.Api.Services.Interfaces;

public interface IPackagingResubmissionApplicationService
{
    Task<List<PackagingResubmissionApplicationDetails?>> GetPackagingResubmissionApplicationDetails(string request);

    Task<PackagingResubmissionMemberResponse> GetPackagingResubmissionMemberDetails(Guid submissionId, string complianceSchemeId);

    Task CreateEventAsync<T>(T @event, Guid submissionId)
        where T : AbstractEvent;
}
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.ProducerValidation;
using WebApiGateway.Core.Models.RegistrationValidation;
using WebApiGateway.Core.Models.Submission;
using WebApiGateway.Core.Models.SubmissionHistory;
using WebApiGateway.Core.Models.Submissions;

namespace WebApiGateway.Api.Services.Interfaces;

public interface ISubmissionService
{
    Task<Guid> CreateSubmissionAsync(SubmissionType submissionType, string submissionPeriod, Guid? complianceSchemeId);

    Task<Guid> CreateAntivirusCheckEventAsync(string fileName, FileType fileType, Guid submissionId, Guid? registrationSetId);

    Task CreateFileDownloadCheckEventAsync(Guid submissionId, FileDownloadCheckEvent fileDownloadCheckEvent);

    Task CreateRegistrationEventAsync(Guid submissionId, RegistrationApplicationPayload applicationPayload);

    Task<HttpResponseMessage> GetSubmissionAsync(Guid submissionId);

    Task<List<AbstractSubmission>> GetSubmissionsAsync(string queryString);

    Task<List<ProducerValidationIssueRow>> GetProducerValidationIssuesAsync(Guid submissionId);

    Task<List<RegistrationValidationError>> GetRegistrationValidationErrorsAsync(Guid submissionId);

    Task SubmitAsync(Guid submissionId, SubmissionPayload submissionPayload);

    Task SubmitAsync(CreateSubmission submission);

    Task<List<SubmissionHistoryResponse>> GetSubmissionPeriodHistory(Guid submissionId, string queryString);

    Task<List<SubmissionGetResponse>> GetSubmissionsByFilter(Guid organisationId, Guid? complianceSchemeId, int? year, SubmissionType submissionType);

    Task<string> GetFileBlobNameAsync(Guid submissionId, Guid fileId);
}
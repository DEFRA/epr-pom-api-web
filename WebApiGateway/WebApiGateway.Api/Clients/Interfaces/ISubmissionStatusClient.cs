using EPR.SubmissionMicroservice.Application.Features.Queries.Common;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.ProducerValidation;
using WebApiGateway.Core.Models.RegistrationValidation;
using WebApiGateway.Core.Models.Submission;
using WebApiGateway.Core.Models.SubmissionHistory;
using WebApiGateway.Core.Models.Submissions;

namespace WebApiGateway.Api.Clients.Interfaces;

public interface ISubmissionStatusClient
{
    Task CreateSubmissionAsync(CreateSubmission submission);

    Task CreateEventAsync(AntivirusCheckEvent @event, Guid submissionId);

    Task CreateEventAsync<T>(T @event, Guid submissionId)
    where T : AbstractEvent;

    Task CreateApplicationSubmittedEventAsync(RegistrationApplicationSubmittedEvent registrationEvent, Guid submissionId);

    Task CreateRegistrationFeePaymentEventAsync(RegistrationFeePaymentEvent registrationEvent, Guid submissionId);

    Task<HttpResponseMessage> GetSubmissionAsync(Guid submissionId);

    Task<List<AbstractSubmission>> GetSubmissionsAsync(string queryString);

    Task<List<RegistrationValidationError>> GetRegistrationValidationErrorsAsync(Guid submissionId);

    Task<List<RegistrationValidationError>> GetRegistrationValidationWarningsAsync(Guid submissionId);

    Task<List<ProducerValidationIssueRow>> GetProducerValidationErrorRowsAsync(Guid submissionId);

    Task<List<ProducerValidationIssueRow>> GetProducerValidationWarningRowsAsync(Guid submissionId);

    Task SubmitAsync(Guid submissionId, SubmissionPayload submissionPayload);

    Task<SubmissionHistoryEventsResponse> GetSubmissionPeriodHistory(Guid submissionId, string queryString);

    Task<List<SubmissionGetResponse>> GetSubmissionsByFilter(Guid organisationId, Guid? complianceSchemeId, int? year, SubmissionType submissionType);

    Task<RegistrationApplicationDetails?> GetRegistrationApplicationDetails(string queryString);

    Task<HttpResponseMessage> CreateFileDownloadEventAsync(FileDownloadCheckEvent fileDownloadCheckEvent, Guid submissionId);

    Task<AntivirusResultEvent> GetFileScanResultAsync(Guid submissionId, Guid fileId);

    Task<List<PackagingResubmissionApplicationDetails?>> GetPackagingResubmissionApplicationDetails(string queryString);
}
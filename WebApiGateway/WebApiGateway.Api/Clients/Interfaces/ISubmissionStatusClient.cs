using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.ProducerValidation;
using WebApiGateway.Core.Models.RegistrationValidation;
using WebApiGateway.Core.Models.Submission;

namespace WebApiGateway.Api.Clients.Interfaces;

public interface ISubmissionStatusClient
{
    Task CreateSubmissionAsync(CreateSubmission submission);

    Task CreateEventAsync(AntivirusCheckEvent @event, Guid submissionId);

    Task<HttpResponseMessage> GetSubmissionAsync(Guid submissionId);

    Task<List<AbstractSubmission>> GetSubmissionsAsync(string queryString);

    Task<List<RegistrationValidationError>> GetRegistrationValidationErrorsAsync(Guid submissionId);

    Task<List<ProducerValidationIssueRow>> GetProducerValidationErrorRowsAsync(Guid submissionId);

    Task<List<ProducerValidationIssueRow>> GetProducerValidationWarningRowsAsync(Guid submissionId);

    Task SubmitAsync(Guid submissionId, SubmissionPayload submissionPayload);
}
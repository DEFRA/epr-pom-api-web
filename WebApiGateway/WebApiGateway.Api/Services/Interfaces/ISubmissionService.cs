using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.ProducerValidation;
using WebApiGateway.Core.Models.RegistrationValidation;
using WebApiGateway.Core.Models.Submission;

namespace WebApiGateway.Api.Services.Interfaces;

public interface ISubmissionService
{
    Task<Guid> CreateSubmissionAsync(SubmissionType submissionType, string submissionPeriod, Guid? complianceSchemeId);

    Task<Guid> CreateAntivirusCheckEventAsync(string fileName, FileType fileType, Guid submissionId, Guid? registrationSetId);

    Task<HttpResponseMessage> GetSubmissionAsync(Guid submissionId);

    Task<List<AbstractSubmission>> GetSubmissionsAsync(string queryString);

    Task<List<ProducerValidationIssueRow>> GetProducerValidationIssuesAsync(Guid submissionId);

    Task<List<RegistrationValidationError>> GetRegistrationValidationErrorsAsync(Guid submissionId);

    Task SubmitAsync(Guid submissionId, SubmissionPayload submissionPayload);
}
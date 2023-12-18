namespace WebApiGateway.Api.Services.Interfaces;

using Core.Enumeration;

public interface IFileUploadService
{
    Task<Guid> UploadFileAsync(
        Stream fileStream,
        SubmissionType submissionType,
        SubmissionSubType? submissionSubType,
        string fileName,
        string submissionPeriod,
        Guid? originalSubmissionId,
        Guid? registrationSetId,
        Guid? complianceSchemeId);
}
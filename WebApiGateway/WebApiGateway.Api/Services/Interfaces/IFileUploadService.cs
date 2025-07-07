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
        Guid? complianceSchemeId,
        bool? isResubmission);

    Task<Guid> UploadFileSubsidiaryAsync(
        Stream fileStream,
        SubmissionType submissionType,
        string fileName,
        Guid? complianceSchemeId);

    Task<Guid> UploadFileAccreditationAsync(
        Stream fileStream,
        SubmissionType submissionType,
        string fileName,
        Guid? originalSubmissionId);
}
namespace WebApiGateway.Api.Services.Interfaces;

using Core.Enumeration;

public interface IFileUploadService
{
    Task<Guid> UploadFileAsync(
        Stream fileStream,
        FileUploadDetails fileUploadDetails);

    Task<Guid> UploadFileSubsidiaryAsync(
        Stream fileStream,
        SubmissionType submissionType,
        string fileName,
        Guid? complianceSchemeId,
        string? registrationJourney);

    Task<Guid> UploadFileAccreditationAsync(
        Stream fileStream,
        SubmissionType submissionType,
        string fileName,
        Guid? originalSubmissionId,
        string? registrationJourney);
}
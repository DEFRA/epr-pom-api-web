using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Helpers;

namespace WebApiGateway.Api.Services;

public class FileUploadService(
    ISubmissionService submissionService,
    IAntivirusService antivirusService)
    : IFileUploadService
{
    public async Task<Guid> UploadFileAsync(
        Stream fileStream,
        FileUploadDetails fileUploadDetails)
    {
        var fileType = fileUploadDetails.SubmissionType is SubmissionType.Producer
            ? FileType.Pom
            : (FileType)Enum.Parse(typeof(FileType), fileUploadDetails.SubmissionSubType.ToString());

        var submissionId = fileType is FileType.Pom or FileType.CompanyDetails && fileUploadDetails.OriginalSubmissionId is null
            ? await submissionService.CreateSubmissionAsync(
                fileUploadDetails.SubmissionType, fileUploadDetails.SubmissionPeriod, fileUploadDetails.ComplianceSchemeId,
                fileUploadDetails.IsResubmission, fileUploadDetails.RegistrationJourney)
            : fileUploadDetails.OriginalSubmissionId.Value;

        var truncatedFileName = FileHelpers.GetTruncatedFileName(fileUploadDetails.FileName, FileConstants.FileNameTruncationLength);
        var fileId = await submissionService.CreateAntivirusCheckEventAsync(
            truncatedFileName, fileType, submissionId, fileUploadDetails.RegistrationSetId);
        await antivirusService.SendFileAsync(fileUploadDetails.SubmissionType, fileId, truncatedFileName, fileStream);

        return submissionId;
    }

    public async Task<Guid> UploadFileSubsidiaryAsync(
        Stream fileStream,
        SubmissionType submissionType,
        string fileName,
        Guid? complianceSchemeId,
        string? registrationJourney)
    {
        var fileType = submissionType is SubmissionType.Subsidiary
            ? FileType.Subsidiaries
            : (FileType)Enum.Parse(typeof(FileType), submissionType.ToString());

        var submissionId = await submissionService.CreateSubmissionAsync(
            submissionType,
            "NA Subsidiary File Upload",
            complianceSchemeId,
            null,
            registrationJourney);
        var truncatedFileName = FileHelpers.GetTruncatedFileName(fileName, FileConstants.FileNameTruncationLength);
        var fileId = await submissionService.CreateAntivirusCheckEventAsync(truncatedFileName, fileType, submissionId, null);
        await antivirusService.SendFileAsync(submissionType, fileId, truncatedFileName, fileStream);

        return submissionId;
    }

    public async Task<Guid> UploadFileAccreditationAsync(
        Stream fileStream,
        SubmissionType submissionType,
        string fileName,
        Guid? originalSubmissionId,
        string? registrationJourney)
    {
        var fileType = submissionType is SubmissionType.Accreditation
            ? FileType.Accreditation
            : (FileType)Enum.Parse(typeof(FileType), submissionType.ToString());

        var submissionId = originalSubmissionId ?? await submissionService.CreateSubmissionAsync(
            submissionType,
            "NA Accreditation File Upload",
            null,
            null,
            registrationJourney);
        var truncatedFileName = FileHelpers.GetTruncatedFileName(fileName, FileConstants.FileNameTruncationLength);
        var fileId = await submissionService.CreateAntivirusCheckEventAsync(truncatedFileName, fileType, submissionId, null);
        await antivirusService.SendFileAsync(submissionType, fileId, truncatedFileName, fileStream);

        return submissionId;
    }
}

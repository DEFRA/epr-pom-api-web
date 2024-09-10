namespace WebApiGateway.Api.Services;

using Core.Constants;
using Core.Enumeration;
using Core.Helpers;
using Interfaces;

public class FileUploadService : IFileUploadService
{
    private readonly ISubmissionService _submissionService;
    private readonly IAntivirusService _antivirusService;

    public FileUploadService(
        ISubmissionService submissionService,
        IAntivirusService antivirusService)
    {
        _submissionService = submissionService;
        _antivirusService = antivirusService;
    }

    public async Task<Guid> UploadFileAsync(
        Stream fileStream,
        SubmissionType submissionType,
        SubmissionSubType? submissionSubType,
        string fileName,
        string submissionPeriod,
        Guid? originalSubmissionId,
        Guid? registrationSetId,
        Guid? complianceSchemeId)
    {
        var fileType = submissionType is SubmissionType.Producer
            ? FileType.Pom
            : (FileType)Enum.Parse(typeof(FileType), submissionSubType.ToString());

        var submissionId = fileType is FileType.Pom or FileType.CompanyDetails && originalSubmissionId is null
            ? await _submissionService.CreateSubmissionAsync(submissionType, submissionPeriod, complianceSchemeId)
            : originalSubmissionId.Value;

        var truncatedFileName = FileHelpers.GetTruncatedFileName(fileName, FileConstants.FileNameTruncationLength);
        var fileId = await _submissionService.CreateAntivirusCheckEventAsync(truncatedFileName, fileType, submissionId, registrationSetId);
        await _antivirusService.SendFileAsync(submissionType, fileId, truncatedFileName, fileStream);

        return submissionId;
    }

    public async Task<Guid> UploadFileSubsidiaryAsync(
        Stream fileStream,
        SubmissionType submissionType,
        string fileName,
        Guid? complianceSchemeId)
    {
        var fileType = submissionType is SubmissionType.Subsidiary
            ? FileType.Subsidiaries
            : (FileType)Enum.Parse(typeof(FileType), submissionType.ToString());

        var submissionId = await _submissionService.CreateSubmissionAsync(submissionType, "NA Subsidiary File Upload", complianceSchemeId);
        var truncatedFileName = FileHelpers.GetTruncatedFileName(fileName, FileConstants.FileNameTruncationLength);
        var fileId = await _submissionService.CreateAntivirusCheckEventAsync(truncatedFileName, fileType, submissionId, null);
        await _antivirusService.SendFileAsync(submissionType, fileId, truncatedFileName, fileStream);

        return submissionId;
    }
}

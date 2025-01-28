﻿using WebApiGateway.Api.Services.Interfaces;
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
            ? await submissionService.CreateSubmissionAsync(submissionType, submissionPeriod, complianceSchemeId)
            : originalSubmissionId.Value;

        var truncatedFileName = FileHelpers.GetTruncatedFileName(fileName, FileConstants.FileNameTruncationLength);
        var fileId = await submissionService.CreateAntivirusCheckEventAsync(truncatedFileName, fileType, submissionId, registrationSetId);
        await antivirusService.SendFileAsync(submissionType, fileId, truncatedFileName, fileStream);

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

        var submissionId = await submissionService.CreateSubmissionAsync(submissionType, "NA Subsidiary File Upload", complianceSchemeId);
        var truncatedFileName = FileHelpers.GetTruncatedFileName(fileName, FileConstants.FileNameTruncationLength);
        var fileId = await submissionService.CreateAntivirusCheckEventAsync(truncatedFileName, fileType, submissionId, null);
        await antivirusService.SendFileAsync(submissionType, fileId, truncatedFileName, fileStream);

        return submissionId;
    }
}

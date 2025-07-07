using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Helpers;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.FileDownload;
using WebApiGateway.Core.Options;

namespace WebApiGateway.Api.Services;

public class FileDownloadService(
    BlobServiceClient blobServiceClient,
    ISubmissionService submissionService,
    IAntivirusService antivirusService,
    IOptions<StorageAccountOptions> options) : IFileDownloadService
{
    private readonly StorageAccountOptions _options = options.Value;

    public async Task<FileDownloadData> DownloadFileAsync(Guid fileId, string fileName, SubmissionType submissionType, Guid submissionId)
    {
        var blobContainer = GetContainerName(submissionType);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(blobContainer);
        var blobName = await submissionService.GetFileBlobNameAsync(submissionId, fileId);
        var blob = blobContainerClient.GetBlobClient(blobName);
        var stream = new MemoryStream();
        await blob.DownloadToAsync(stream);

        var truncatedFileName = FileHelpers.GetTruncatedFileName(fileName, FileConstants.FileNameTruncationLength);
        var antiVirusResponse = await antivirusService.SendFileAndScanAsync(submissionType, fileId, truncatedFileName, stream);
        var antiVirusResult = await antiVirusResponse.Content.ReadAsStringAsync();

        var fileDownloadCheckEvent = new FileDownloadCheckEvent()
        {
            ContentScan = antiVirusResult,
            FileId = fileId,
            FileName = fileName,
            BlobName = fileId.ToString(),
            SubmissionId = submissionId,
            SubmissionType = submissionType
        };

        await submissionService.CreateFileDownloadCheckEventAsync(submissionId, fileDownloadCheckEvent);

        return new FileDownloadData
        {
            Stream = stream,
            AntiVirusResult = antiVirusResult
        };
    }

    private string GetContainerName(SubmissionType submissionType) => submissionType switch
    {
        SubmissionType.Producer => _options.PomContainer,
        SubmissionType.Accreditation => _options.AccreditationContainer,
        _ => _options.RegistrationContainer
    };
}

namespace WebApiGateway.Api.Services.Interfaces;

using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.FileDownload;

public interface IFileDownloadService
{
    Task<FileDownloadData> DownloadFileAsync(Guid fileId, string fileName, SubmissionType submissionType, Guid submissionId);
}

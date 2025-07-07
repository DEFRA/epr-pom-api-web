namespace WebApiGateway.Api.Controllers;

using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using WebApiGateway.Api.Constants;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Enumeration;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/file-download")]
public class FileDownloadController(IFileDownloadService fileDownloadService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([Required] string fileName, [Required] Guid fileId, [Required] SubmissionType submissionType, [Required] Guid submissionId)
    {
        var fileData = await fileDownloadService.DownloadFileAsync(fileId, fileName, submissionType, submissionId);

        if (fileData.AntiVirusResult == ContentScan.Clean)
        {
            string contentType = GetContentType(submissionType, fileName);
            return File(fileData.Stream.ToArray(), contentType, fileName);
        }

        return new ObjectResult("The file was found but it was flagged as infected. It will not be downloaded.") { StatusCode = StatusCodes.Status403Forbidden };
    }

    private static string GetContentType(SubmissionType submissionType, string fileName)
    {
        var contentType = "text/csv";

        if (submissionType == SubmissionType.Accreditation)
        {
            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(fileName, out contentType))
            {
                contentType = "application/octet-stream";
            }
        }

        return contentType;
    }
}

namespace WebApiGateway.Api.Controllers;

using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
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
            return File(fileData.Stream.ToArray(), "text/csv", fileName);
        }

        return new ObjectResult("The file was found but it was flagged as infected. It will not be downloaded.") { StatusCode = StatusCodes.Status403Forbidden };
    }
}

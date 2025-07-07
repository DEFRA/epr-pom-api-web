using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/file-upload-accreditation")]
public class FileUploadAccreditationController(IFileUploadService fileUploadService) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(FileConstants.MaxFileSizeInBytes)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FileUploadAccreditation(
        [FromHeader][Required] string fileName,
        [FromHeader][Required] SubmissionType submissionType,
        [FromHeader] Guid? submissionId)
    {
        ValidateUploadSubmission(fileName, submissionType);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(statusCode: 400);
        }

        var id = await fileUploadService.UploadFileAccreditationAsync(
            Request.Body,
            submissionType,
            fileName,
            submissionId);

        return new CreatedAtRouteResult(nameof(SubmissionController.GetSubmission), new { submissionId = id }, null);
    }

    private void ValidateUploadSubmission(string fileName, SubmissionType submissionType)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            ModelState.AddModelError(nameof(fileName), $"{nameof(fileName)} header is required");
        }

        if (submissionType is not SubmissionType.Accreditation)
        {
            ModelState.AddModelError(nameof(submissionType), $"{nameof(submissionType)} header must be accreditation");
        }
    }
}

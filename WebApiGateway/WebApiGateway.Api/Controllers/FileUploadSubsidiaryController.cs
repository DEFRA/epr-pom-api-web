using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/file-upload-subsidiary")]
public class FileUploadSubsidiaryController(IFileUploadService fileUploadService, ISubsidiaryService subsidiaryService)
    : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(FileConstants.MaxFileSizeInBytes)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FileUploadSubsidiary(
        [FromHeader][Required] string fileName,
        [FromHeader][Required] SubmissionType submissionType,
        [FromHeader] Guid? complianceSchemeId)
    {
        ValidateUploadSubmission(fileName, submissionType);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(statusCode: 400);
        }

        await subsidiaryService.InitializeUploadStatusAsync();

        var id = await fileUploadService.UploadFileSubsidiaryAsync(
            Request.Body,
            submissionType,
            fileName,
            complianceSchemeId);

        return new CreatedAtRouteResult(nameof(SubmissionController.GetSubmission), new { submissionId = id }, null);
    }

    private void ValidateUploadSubmission(string fileName, SubmissionType submissionType)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            ModelState.AddModelError(nameof(fileName), $"{nameof(fileName)} header is required");
        }

        if (submissionType is not SubmissionType.Subsidiary)
        {
            ModelState.AddModelError(nameof(submissionType), $"{nameof(submissionType)} header must be subsidiary");
        }
    }
}
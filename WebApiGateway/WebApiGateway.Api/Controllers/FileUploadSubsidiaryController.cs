namespace WebApiGateway.Api.Controllers;

using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Core.Constants;
using Core.Enumeration;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/file-upload-subsidiary")]
public class FileUploadSubsidiaryController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;

    public FileUploadSubsidiaryController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    [HttpPost]
    [RequestSizeLimit(FileConstants.MaxFileSizeInBytes)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FileUploadSubsidiary(
        [FromHeader][Required] string fileName,
        [FromHeader][Required] SubmissionType submissionType)
    {
        ValidateUploadSubmission(fileName, submissionType);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(statusCode: 400);
        }

        var id = await _fileUploadService.UploadFileSubsidiaryAsync(
            Request.Body,
            submissionType,
            fileName);

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
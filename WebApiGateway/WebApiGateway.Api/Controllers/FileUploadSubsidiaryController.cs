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
    private readonly ISubsidiariesService _subsidiariesService;

    public FileUploadSubsidiaryController(IFileUploadService fileUploadService, ISubsidiariesService subsidiariesService)
    {
        _fileUploadService = fileUploadService;
        _subsidiariesService = subsidiariesService;
    }

    [HttpGet("template")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFileUploadTemplateAsync()
    {
        var file = await _subsidiariesService.GetFileUploadTemplateAsync();

        if (file == null)
        {
            return NotFound();
        }

        return File(file.Content, file.ContentType, file.Name);
    }

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

        var id = await _fileUploadService.UploadFileSubsidiaryAsync(
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
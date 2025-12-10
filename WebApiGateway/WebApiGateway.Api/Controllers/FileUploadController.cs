using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/file-upload")]
public class FileUploadController(IFileUploadService fileUploadService) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(FileConstants.MaxFileSizeInBytes)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FileUpload(
        [FromHeader] [Required] string fileName,
        [FromHeader] [Required] SubmissionType submissionType,
        [FromHeader] SubmissionSubType? submissionSubType,
        [FromHeader] Guid? registrationSetId,
        [FromHeader] [Required] string submissionPeriod,
        [FromHeader] Guid? submissionId,
        [FromHeader] Guid? complianceSchemeId,
        [FromHeader] bool? isResubmission,
        [FromHeader] string? registrationJourney)
    {
        if (submissionType is SubmissionType.Registration)
        {
            ValidateRegistrationSubmission(submissionSubType, submissionId, registrationSetId);
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(statusCode: 400);
        }

        var fileUploadDetails = new FileUploadDetails
        {
            FileName = fileName,
            SubmissionType = submissionType,
            SubmissionSubType = submissionSubType,
            RegistrationSetId = registrationSetId,
            SubmissionPeriod = submissionPeriod,
            OriginalSubmissionId = submissionId,
            ComplianceSchemeId = complianceSchemeId,
            IsResubmission = isResubmission,
            RegistrationJourney = registrationJourney
        };

        var id = await fileUploadService.UploadFileAsync(
            Request.Body,
            fileUploadDetails);

        return new CreatedAtRouteResult(nameof(SubmissionController.GetSubmission), new { submissionId = id }, null);
    }

    private void ValidateRegistrationSubmission(SubmissionSubType? submissionSubType, Guid? submissionId,
        Guid? registrationSetId)
    {
        if (submissionSubType is null)
        {
            ModelState.AddModelError(nameof(submissionSubType), $"{nameof(submissionSubType)} header is required");
        }

        if (submissionSubType is SubmissionSubType.Partnerships or SubmissionSubType.Brands && submissionId is null)
        {
            ModelState.AddModelError(nameof(submissionId), $"{nameof(submissionId)} header is required");
        }

        if (registrationSetId is null)
        {
            ModelState.AddModelError(nameof(registrationSetId), $"{nameof(registrationSetId)} header is required");
        }
    }
}
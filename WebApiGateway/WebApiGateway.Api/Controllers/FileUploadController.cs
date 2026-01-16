using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Controllers.Requests;
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
    public async Task<IActionResult> FileUpload(FileUploadRequest request)
    {
        if (request.SubmissionType is SubmissionType.Registration)
        {
            ValidateRegistrationSubmission(request.SubmissionSubType, request.SubmissionId, request.RegistrationSetId);
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(statusCode: 400);
        }

        var fileUploadDetails = new FileUploadDetails
        {
            FileName = request.FileName,
            SubmissionType = request.SubmissionType,
            SubmissionSubType = request.SubmissionSubType,
            RegistrationSetId = request.RegistrationSetId,
            SubmissionPeriod = request.SubmissionPeriod,
            OriginalSubmissionId = request.SubmissionId,
            ComplianceSchemeId = request.ComplianceSchemeId,
            IsResubmission = request.IsResubmission,
            RegistrationJourney = request.RegistrationJourney
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
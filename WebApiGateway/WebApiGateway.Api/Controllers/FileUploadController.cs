﻿namespace WebApiGateway.Api.Controllers;

using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Core.Constants;
using Core.Enumeration;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/file-upload")]
public class FileUploadController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;

    public FileUploadController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    [HttpPost]
    [RequestSizeLimit(FileConstants.MaxFileSizeInBytes)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FileUpload(
        [FromHeader][Required] string fileName,
        [FromHeader][Required] SubmissionType submissionType,
        [FromHeader] SubmissionSubType? submissionSubType,
        [FromHeader] Guid? registrationSetId,
        [FromHeader][Required] string submissionPeriod,
        [FromHeader] Guid? submissionId,
        [FromHeader] Guid? complianceSchemeId)
    {
        if (submissionType is SubmissionType.Registration)
        {
            ValidateRegistrationSubmission(submissionSubType, submissionId, registrationSetId);
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(statusCode: 400);
        }

        var id = await _fileUploadService.UploadFileAsync(
            Request.Body,
            submissionType,
            submissionSubType,
            fileName,
            submissionPeriod,
            submissionId,
            registrationSetId,
            complianceSchemeId);

        return new CreatedAtRouteResult(nameof(SubmissionController.GetSubmission), new { submissionId = id }, null);
    }

    private void ValidateRegistrationSubmission(SubmissionSubType? submissionSubType, Guid? submissionId, Guid? registrationSetId)
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
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Api.Controllers.Requests;

public record FileUploadRequest(
    [FromHeader] [Required] string FileName,
    [FromHeader] [Required] SubmissionType SubmissionType,
    [FromHeader] SubmissionSubType? SubmissionSubType,
    [FromHeader] Guid? RegistrationSetId,
    [FromHeader] [Required] string SubmissionPeriod,
    [FromHeader] Guid? SubmissionId,
    [FromHeader] Guid? ComplianceSchemeId,
    [FromHeader] bool? IsResubmission,
    [FromHeader] string? RegistrationJourney);
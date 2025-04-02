using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Submission;
using WebApiGateway.Core.Models.Submissions;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/submissions")]
public class SubmissionController(
    ISubmissionService submissionService) : ControllerBase
{
    [HttpGet("{submissionId:guid}", Name = nameof(GetSubmission))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubmission([FromRoute] Guid submissionId)
    {
        var response = await submissionService.GetSubmissionAsync(submissionId);
        var responseType = response.Content.Headers.GetContentType();
        var responseBody = await response.Content.ReadAsStringAsync();

        return new ContentResult
        {
            StatusCode = (int)response.StatusCode,
            Content = responseBody,
            ContentType = responseType,
        };
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubmissions()
    {
        var submissions = await submissionService.GetSubmissionsAsync(Request.QueryString.Value);
        return new OkObjectResult(submissions);
    }

    [HttpGet("submission-history/{submissionId:guid}", Name = nameof(GetSubmissionHistory))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubmissionHistory([FromRoute] Guid submissionId)
    {
        var submissions = await submissionService.GetSubmissionPeriodHistory(submissionId, Request.QueryString.Value);
        return new OkObjectResult(submissions);
    }

    [HttpGet("submission-Ids/{organisationId:guid}", Name = nameof(GetSubmissionByFilter))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubmissionByFilter([FromRoute] Guid organisationId, [FromQuery] SubmissionGetRequest request)
    {
        var submissions = await submissionService.GetSubmissionsByFilter(
            organisationId,
            request.ComplianceSchemeId,
            request.Year,
            request.Type);

        return new OkObjectResult(submissions);
    }

    /// <summary>
    /// Gets organisation details errors & warnings.
    /// </summary>
    /// <param name="submissionId">submission id.</param>
    /// <returns>list of registeration validation issues i.e. errors & warnings.</returns>
    [HttpGet("{submissionId:guid}/organisation-details-errors", Name = nameof(GetRegistrationValidationErrors))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegistrationValidationErrors([FromRoute] Guid submissionId)
    {
        var registrationValidationIssues = await submissionService.GetRegistrationValidationErrorsAsync(submissionId);
        return new OkObjectResult(registrationValidationIssues);
    }

    [HttpGet("{submissionId:guid}/producer-validations", Name = nameof(GetProducerValidationIssues))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducerValidationIssues([FromRoute] Guid submissionId)
    {
        var producerValidationIssues = await submissionService.GetProducerValidationIssuesAsync(submissionId);
        return new OkObjectResult(producerValidationIssues);
    }

    [HttpPost("{submissionId:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Submit([FromRoute] Guid submissionId, [FromBody] SubmissionPayload submissionPayload)
    {
        await submissionService.SubmitAsync(submissionId, submissionPayload);
        return new NoContentResult();
    }

    [HttpPost("{submissionId:guid}/submit-registration-application")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SubmitRegistrationApplication([FromRoute] Guid submissionId, [FromBody] RegistrationApplicationPayload applicationPayload)
    {
        await submissionService.CreateRegistrationEventAsync(submissionId, applicationPayload);
        return new NoContentResult();
    }
}
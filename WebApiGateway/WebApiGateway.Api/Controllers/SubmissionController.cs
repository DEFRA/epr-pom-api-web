using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.ComplianceSchemeDetails;
using WebApiGateway.Core.Models.Submission;
using WebApiGateway.Core.Models.Submissions;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/submissions")]
public class SubmissionController(
    ISubmissionService submissionService,
    IProducerDetailsService producerDetailsService,
    IComplianceSchemeDetailsService complianceSchemeDetailsService) : ControllerBase
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

    [HttpGet("{submissionId:guid}/organisation-details-errors", Name = nameof(GetRegistrationValidationErrors))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegistrationValidationErrors([FromRoute] Guid submissionId)
    {
        var registrationValidationErrors = await submissionService.GetRegistrationValidationErrorsAsync(submissionId);
        return new OkObjectResult(registrationValidationErrors);
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

    [HttpPost("create-submission")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Submit([FromBody] CreateSubmission submission)
    {
        await submissionService.SubmitAsync(submission);
        return new NoContentResult();
    }

    [HttpPost("{submissionId:guid}/submit-registration-application")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SubmitRegistrationApplication([FromRoute] Guid submissionId, [FromBody] RegistrationApplicationPayload applicationPayload)
    {
        await submissionService.CreateRegistrationEventAsync(submissionId, applicationPayload);
        return new NoContentResult();
    }

    [HttpGet("get-registration-application-details")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetRegistrationApplicationDetails([FromQuery] int organisationNumber, [FromQuery] Guid? complianceSchemeId)
    {
        var result = await submissionService.GetRegistrationApplicationDetails(Request.QueryString.Value);
        if (result is null)
        {
            return NoContent();
        }

        var producerDetails = await producerDetailsService.GetProducerDetails(organisationNumber);
        result.ProducerDetails = producerDetails ?? null!;

        result.CsoMemberDetails = Enumerable.Empty<GetComplianceSchemeMemberDetailsResponse>().ToList();

        if (complianceSchemeId.HasValue)
        {
            var csoMemberDetails = await complianceSchemeDetailsService.GetComplianceSchemeDetails(organisationNumber, complianceSchemeId.Value);
            result.CsoMemberDetails = csoMemberDetails is null || csoMemberDetails.Count == 0 ? result.CsoMemberDetails : csoMemberDetails;
        }

        return Ok(result);
    }
}
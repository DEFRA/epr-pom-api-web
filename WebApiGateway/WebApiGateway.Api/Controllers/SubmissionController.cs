using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Submission;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/submissions")]
public class SubmissionController : ControllerBase
{
    private readonly ISubmissionService _submissionService;

    public SubmissionController(ISubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    [HttpGet("{submissionId:guid}", Name = nameof(GetSubmission))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubmission([FromRoute] Guid submissionId)
    {
        var response = await _submissionService.GetSubmissionAsync(submissionId);
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
        var submissions = await _submissionService.GetSubmissionsAsync(Request.QueryString.Value);
        return new OkObjectResult(submissions);
    }

    [HttpGet("{submissionId:guid}/producer-validations", Name = nameof(GetProducerValidationIssues))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducerValidationIssues([FromRoute] Guid submissionId)
    {
        var producerValidationIssues = await _submissionService.GetProducerValidationIssuesAsync(submissionId);
        return new OkObjectResult(producerValidationIssues);
    }

    [HttpPost("{submissionId:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Submit(
        [FromRoute] Guid submissionId,
        [FromBody] SubmissionPayload submissionPayload)
    {
        await _submissionService.SubmitAsync(submissionId, submissionPayload);
        return new NoContentResult();
    }
}
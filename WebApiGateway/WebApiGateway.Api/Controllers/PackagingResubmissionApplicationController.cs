using Asp.Versioning;
using EPR.SubmissionMicroservice.Application.Features.Queries.Common;
using EPR.SubmissionMicroservice.Data.Entities.SubmissionEvent;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.PackagingResubmissionApplication;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/packaging-resubmission")]
public class PackagingResubmissionApplicationController(
        IPackagingResubmissionApplicationService packagingResubmissionApplicationService)
        : ControllerBase
{
    [HttpGet("get-application-details")]
    [ProducesResponseType(typeof(List<PackagingResubmissionApplicationDetails>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetPackagingResubmissionApplicationDetails()
    {
        var result = await packagingResubmissionApplicationService.GetPackagingResubmissionApplicationDetails(Request.QueryString.Value);

        return result is not null ? Ok(result) : NoContent();
    }

    [HttpGet("get-resubmission-member-details/{submissionId:guid}")]
    [ProducesResponseType(typeof(PackagingResubmissionMemberResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<IActionResult> GetPackagingResubmissionMemberDetails([FromRoute] Guid submissionId, [FromQuery] string? complianceSchemeId)
    {
        var result = await packagingResubmissionApplicationService.GetPackagingResubmissionMemberDetails(submissionId, complianceSchemeId);

        if (result is not null && !string.IsNullOrEmpty(result.ErrorMessage))
        {
            return StatusCode(StatusCodes.Status428PreconditionRequired, result.ErrorMessage);
        }

        return result is not null ? Ok(result) : NoContent();
    }

    [HttpPost("{submissionId:guid}/create-packaging-resubmission-reference-number-event")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreatePackagingResubmissionReferenceNumberEvent([FromRoute] Guid submissionId, [FromBody] PackagingResubmissionReferenceNumberCreatedEvent @event)
    {
        await packagingResubmissionApplicationService.CreateEventAsync<PackagingResubmissionReferenceNumberCreatedEvent>(@event, submissionId);
        return new NoContentResult();
    }

    [HttpPost("{submissionId:guid}/create-packaging-resubmission-fee-view-event")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreatePackagingResubmissionFeeViewEvent([FromRoute] Guid submissionId, [FromBody] PackagingResubmissionFeeViewCreatedEvent @event)
    {
        await packagingResubmissionApplicationService.CreateEventAsync<PackagingResubmissionFeeViewCreatedEvent>(@event, submissionId);
        return new NoContentResult();
    }

    [HttpPost("{submissionId:guid}/create-packaging-resubmission-fee-payment-event")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreatePackagingResubmissionPaymentMethodEvent([FromRoute] Guid submissionId, [FromBody] PackagingDataResubmissionFeePaymentEvent @event)
    {
        await packagingResubmissionApplicationService.CreateEventAsync<PackagingDataResubmissionFeePaymentEvent>(@event, submissionId);
        return new NoContentResult();
    }

    [HttpPost("{submissionId:guid}/create-packaging-resubmission-application-submitted-event")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreatePackagingResubmissionSubmittedEvent([FromRoute] Guid submissionId, [FromBody] PackagingResubmissionApplicationSubmittedCreatedEvent @event)
    {
        await packagingResubmissionApplicationService.CreateEventAsync<PackagingResubmissionApplicationSubmittedCreatedEvent>(@event, submissionId);
        return new NoContentResult();
    }
}
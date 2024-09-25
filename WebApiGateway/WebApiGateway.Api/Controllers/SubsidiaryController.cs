using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/subsidiary")]
public class SubsidiaryController : ControllerBase
{
    private const string SubsidiaryBulkUploadErrors = "Subsidiary bulk upload errors";
    private const string SubsidiaryBulkUploadProgress = "Subsidiary bulk upload progress";
    private readonly ISubsidiaryService _subsidiaryService;
    private readonly ILogger<SubsidiaryController> _logger;

    public SubsidiaryController(ISubsidiaryService subsidiaryService, ILogger<SubsidiaryController> logger)
    {
        _subsidiaryService = subsidiaryService;
        _logger = logger;
    }

    [HttpGet("{userId:guid}/{organisationId:guid}", Name = nameof(GetNotificationErrors))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotificationErrors([FromRoute] Guid userId, [FromRoute] Guid organisationId)
    {
        try
        {
            var notificationErrors = await _subsidiaryService.GetNotificationErrorsAsync($"{userId}{organisationId}{SubsidiaryBulkUploadErrors}");
            var notificationStatus = await _subsidiaryService.GetNotificationStatusAsync($"{userId}{organisationId}{SubsidiaryBulkUploadProgress}");

            notificationErrors.Status = notificationStatus;

            return new OkObjectResult(notificationErrors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during get notification errors from Redis.");
            return new BadRequestResult();
        }
    }
}

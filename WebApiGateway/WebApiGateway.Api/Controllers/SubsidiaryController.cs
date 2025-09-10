using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Models.Subsidiary;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/subsidiary")]
public class SubsidiaryController(ISubsidiaryService subsidiaryService, ILogger<SubsidiaryController> logger)
    : ControllerBase
{
    [HttpGet("{userId:guid}/{organisationId:guid}", Name = nameof(GetNotificationErrors))]
    [ProducesResponseType(typeof(UploadFileErrorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotificationErrors([FromRoute] Guid userId, [FromRoute] Guid organisationId)
    {
        try
        {
            var notificationErrors = await subsidiaryService.GetNotificationErrorsAsync($"{userId}{organisationId}{SubsidiaryBulkUploadStatusKeys.SubsidiaryBulkUploadErrors}");
            var notificationStatus = await subsidiaryService.GetNotificationStatusAsync($"{userId}{organisationId}{SubsidiaryBulkUploadStatusKeys.SubsidiaryBulkUploadProgress}");
            var notificationRowsAdded = await subsidiaryService.GetNotificationStatusAsync($"{userId}{organisationId}{SubsidiaryBulkUploadStatusKeys.SubsidiaryBulkUploadRowsAdded}");

            notificationErrors.Status = notificationStatus;
            notificationErrors.RowsAdded = int.TryParse(notificationRowsAdded, out var rowsAdded) ? rowsAdded : null;

            return new OkObjectResult(notificationErrors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during get notification errors from Redis.");
            return new BadRequestResult();
        }
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Logging;
using WebApiGateway.Core.Models.Submission;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/registration")]
public class RegistrationApplicationController(
    IRegistrationApplicationService registrationApplicationService,
    ILogger<RegistrationApplicationController> logger)
    : ControllerBase
{
    [HttpGet("get-registration-application-details")]
    [ProducesResponseType(typeof(RegistrationApplicationDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetRegistrationApplicationDetails()
    {
        using (logger.BeginScope("HTTP"))
        using (logger.BeginScope("Getting registration application details"))
        using (logger.AddScopedData(new Dictionary<string, object>
               {
                   ["Querystring"] = Request.QueryString.Value
               }))
        {
            var result = await registrationApplicationService.GetRegistrationApplicationDetails(Request.QueryString.Value);

            return result is not null ? Ok(result) : NoContent();
        }
    }
}
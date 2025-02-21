using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/registration")]
public class RegistrationApplicationController(
    IRegistrationApplicationService registrationApplicationService)
    : ControllerBase
{
    [HttpGet("get-registration-application-details")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetRegistrationApplicationDetails()
    {
        var result = await registrationApplicationService.GetRegistrationApplicationDetails(Request.QueryString.Value);

        return result is not null ? Ok(result) : NoContent();
    }
}
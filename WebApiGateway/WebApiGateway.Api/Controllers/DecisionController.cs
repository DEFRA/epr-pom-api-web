using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/decisions")]
[AllowAnonymous]
public class DecisionController : ControllerBase
{
    private readonly IDecisionService _decisionService;

    public DecisionController(IDecisionService decisionService)
    {
        _decisionService = decisionService;
    }

    [HttpGet]

    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDecision()
    {
        var decision = await _decisionService.GetDecisionAsync(Request.QueryString.Value);
        return new OkObjectResult(decision);
    }
}
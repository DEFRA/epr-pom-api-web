using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.ProducerDetails;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/producer-details")]
public class ProducerDetailsController(IProducerDetailsService producerDetailsService, ILogger<ProducerDetailsController> logger, IConfiguration configuration) : ControllerBase
{
    private readonly string logPrefix = configuration["LogPrefix"];

    [HttpGet("get-producer-details/{organisationId:int}", Name = nameof(GetProducerDetails))]
    [ProducesResponseType(typeof(GetProducerDetailsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducerDetails([FromRoute] int organisationId)
    {
        logger.LogInformation("{Logprefix}: ProducerDetailsController - GetProducerDetails: Get producer details by id {Id}", logPrefix, organisationId);
        return new OkObjectResult(await producerDetailsService.GetProducerDetails(organisationId));
    }
}
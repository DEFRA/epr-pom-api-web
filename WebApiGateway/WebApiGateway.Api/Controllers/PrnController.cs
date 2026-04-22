using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApiGateway.Api.Attributes;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Pagination;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[ComplianceSchemeIdFilter]
public class PrnController(IPrnService prnService, ILogger<PrnController> logger, IConfiguration config, IWasteObligationsProxy wasteObligationsProxy) : ControllerBase
{
    private readonly string logPrefix = config["LogPrefix"];

    [HttpGet("prn/organisation")]
    [ProducesResponseType(typeof(List<PrnModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPrnsForOrganisation()
    {
        logger.LogInformation("{Logprefix}: PrnController - GetAllPrnsForOrganisation: Get AllPrns For logged in User's Organisation", logPrefix);
        return new OkObjectResult(await prnService.GetAllPrnsForOrganisation());
    }

    [HttpGet("prn/{id}")]
    [ProducesResponseType(typeof(PrnModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrnById(Guid id)
    {
        logger.LogInformation("{Logprefix}: PrnController - GetPrnById: Get Prn for given Id {Id}", logPrefix, id);
        return new OkObjectResult(await prnService.GetPrnById(id));
    }

    [HttpGet("prn/search")]
    [ProducesResponseType(typeof(PaginatedResponse<PrnModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPrn([FromQuery] PaginatedRequest request)
    {
        logger.LogInformation("{Logprefix}: PrnController - SearchPrn: Search Prns for given serach criteria {SearchCriteria}", logPrefix, JsonConvert.SerializeObject(request));
        return new OkObjectResult(await prnService.GetSearchPrns(request));
    }

    [HttpGet("prn/obligationcalculation/{year}")]
    [ProducesResponseType(typeof(List<ObligationModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetObligationCalculation(int year)
    {
        logger.LogInformation("{Logprefix}: PrnController - GetObligationCalculation: Get Obligation calculation for year {Year}", logPrefix, year);
        return new OkObjectResult(await prnService.GetObligationCalculationByYearAsync(year));
    }

    [HttpPost("prn/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePrnStatusToAccepted(List<UpdatePrnStatus> updatePrns)
    {
        logger.LogInformation("{Logprefix}: PrnController - UpdatePrnStatusToAccepted: Update Prn Satus for given Prns {Prns}", logPrefix, JsonConvert.SerializeObject(updatePrns));
        await prnService.UpdatePrnStatus(updatePrns);
        return NoContent();
    }

    [HttpGet("prn/compliance-declarations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComplianceDeclarations(int obligationYear, CancellationToken cancellationToken)
    {
        var content = await wasteObligationsProxy.GetComplianceDeclarations(obligationYear, cancellationToken);
        if (content is not null)
        {
            return Content(content, "application/json");
        }

        return NotFound();
    }
}

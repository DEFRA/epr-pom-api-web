using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApiGateway.Api.Attributes;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Pagination;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[ComplianceSchemeIdFilter]
public class PrnController : ControllerBase
{
    private readonly IPrnService _prnService;
    private readonly ILogger<PrnController> _logger;
    private readonly string logPrefix;

    public PrnController(IPrnService prnService, ILogger<PrnController> logger, IConfiguration config)
    {
        _prnService = prnService;
        _logger = logger;
        logPrefix = config["LogPrefix"];
    }

    [HttpGet("prn/organisation")]
    [ProducesResponseType(typeof(List<PrnModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPrnsForOrganisation()
    {
        _logger.LogInformation("{Logprefix}: PrnController - GetAllPrnsForOrganisation: Get AllPrns For logged in User's Organisation", logPrefix);
        return new OkObjectResult(await _prnService.GetAllPrnsForOrganisation());
    }

    [HttpGet("prn/{id}")]
    [ProducesResponseType(typeof(PrnModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrnById(Guid id)
    {
        _logger.LogInformation("{Logprefix}: PrnController - GetPrnById: Get Prn for given Id {Id}", logPrefix, id);
        return new OkObjectResult(await _prnService.GetPrnById(id));
    }

    [HttpGet("prn/obligation/{year}")]
    [ProducesResponseType(typeof(List<ObligationModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetObligation(int year)
    {
        _logger.LogInformation("{Logprefix}: PrnController - GetObligation: Get Obligation request for user organisation and year {Year}", logPrefix, year);
        return new OkObjectResult(await _prnService.GetObligationCalculationByYear(year));
    }

    [HttpGet("prn/search")]
    [ProducesResponseType(typeof(PaginatedResponse<PrnModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPrn([FromQuery] PaginatedRequest request)
    {
        _logger.LogInformation("{Logprefix}: PrnController - SearchPrn: Search Prns for given serach criteria {SearchCriteria}", logPrefix, JsonConvert.SerializeObject(request));
        return new OkObjectResult(await _prnService.GetSearchPrns(request));
    }

    [HttpPost("prn/status")]
    public async Task<IActionResult> UpdatePrnStatusToAccepted(List<UpdatePrnStatus> updatePrns)
    {
        _logger.LogInformation("{Logprefix}: PrnController - UpdatePrnStatusToAccepted: Update Prn Satus for given Prns {Prns}", logPrefix, JsonConvert.SerializeObject(updatePrns));
        await _prnService.UpdatePrnStatus(updatePrns);
        return NoContent();
    }
}

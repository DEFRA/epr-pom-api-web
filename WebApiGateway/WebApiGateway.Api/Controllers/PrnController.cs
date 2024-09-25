using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Pagination;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
    public class PrnController : Controller
    {
        private readonly IPrnService _prnService;
        private readonly ILogger<PrnController> _logger;

        public PrnController(IPrnService prnService, ILogger<PrnController> logger)
        {
            _prnService = prnService;
            _logger = logger;
        }

        [HttpGet("prn/organisation")]
        [ProducesResponseType(typeof(List<PrnModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPrnsForOrganisation()
        {
            return new OkObjectResult(await _prnService.GetAllPrnsForOrganisation());
        }

        [HttpGet("prn/{id}")]
        [ProducesResponseType(typeof(PrnModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPrnById(Guid id)
        {
            _logger.LogDebug("Recieved GetPrnById request for prn {Id}", id);
            return new OkObjectResult(await _prnService.GetPrnById(id));
        }

        [HttpGet("prn/obligation/{id}")]
        [ProducesResponseType(typeof(List<ObligationCalculation>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetObligation(int id)
        {
            _logger.LogDebug("Recieved Get Obligation request for organisation {Id}", id);
            return new OkObjectResult(await _prnService.GetObligationCalculationsByOrganisationId(id));
        }

        [HttpGet("prn/search")]
        [ProducesResponseType(typeof(PaginatedResponse<PrnModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchPrn([FromQuery] PaginatedRequest request)
        {
            return new OkObjectResult(await _prnService.GetSearchPrns(request));
        }

        [HttpPost("prn/status")]
        public async Task<IActionResult> UpdatePrnStatusToAccepted(List<UpdatePrnStatus> updatePrns)
        {
            await _prnService.UpdatePrnStatus(updatePrns);
            return NoContent();
        }
    }
}

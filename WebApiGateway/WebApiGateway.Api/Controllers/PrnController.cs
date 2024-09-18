using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;
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
            _logger.LogDebug("Recieved GetPrnById request for prn {id}", id);
            return new OkObjectResult(await _prnService.GetPrnById(id));
        }

        [HttpPost("prn/status")]
        public async Task<IActionResult> UpdatePrnStatusToAccepted(List<UpdatePrnStatus> updatePrns)
        {
            await _prnService.UpdatePrnStatus(updatePrns);
            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;

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

        [HttpGet("prn/organisation/{organisationId}")]
        public async Task<IActionResult> GetAllPrnsForOrganisation(Guid organisationId)
        {
            return new OkObjectResult(await _prnService.GetAllPrnsForOrganisation(organisationId));
        }

        [HttpGet("prn/{id}")]
        public async Task<IActionResult> GetPrnById(Guid id)
        {
            return new OkObjectResult(await _prnService.GetPrnById(id));
        }

        [HttpPatch("prn/status/{id}")]
        public async Task<IActionResult> UpdatePrnStatusToAccepted(Guid id)
        {
            await _prnService.UpdatePrnStatusToAccepted(id);
            return NoContent();
        }
    }
}

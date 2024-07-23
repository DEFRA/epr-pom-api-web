using Microsoft.AspNetCore.Mvc;
using WebApiGateway.Api.Services.Interfaces;

namespace WebApiGateway.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/prns")]
    public class PrnController : Controller
    {
        private readonly IPrnService _prnService;
        private readonly ILogger<PrnController> _logger;

        public PrnController(IPrnService prnService, ILogger<PrnController> logger)
        {
            _prnService = prnService;
            _logger = logger;
        }

        [HttpGet("organisation/{organisationId}")]
        public async Task<IActionResult> GetAllPrnsForOrganisation(Guid organisationId)
        {
            return new OkObjectResult(await _prnService.GetAllPrnsForOrganisation(organisationId));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllPrnsById(int id)
        {
            return new OkObjectResult(await _prnService.GetPrnById(id));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePrnStatusToAccepted(int id)
        {
            await _prnService.UpdatePrnStatusToAccepted(id);
            return NoContent();
        }
    }
}

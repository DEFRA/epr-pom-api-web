using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Services
{
    public class PrnService : IPrnService
    {
        private readonly IPrnServiceClient _prnServiceClient;
        private readonly ILogger<PrnService> _logger;

        public PrnService(IPrnServiceClient prnServiceClient, ILogger<PrnService> logger)
        {
            _prnServiceClient = prnServiceClient;
            _logger = logger;
        }

        public async Task<List<PrnModel>> GetAllPrnsForOrganisation()
        {
            return await _prnServiceClient.GetAllPrnsForOrganisation();
        }

        public async Task<PrnModel> GetPrnById(Guid id)
        {
            _logger.LogDebug("GetPrnById for prn {prnId}", id);
            return await _prnServiceClient.GetPrnById(id);
        }

        public async Task UpdatePrnStatus(List<UpdatePrnStatus> updatePrns)
        {
            await _prnServiceClient.UpdatePrnStatus(updatePrns);
        }

        public async Task<List<ObligationCalculation>> GetObligationCalculationsByOrganisationId(int organisationId)
        {
            return await _prnServiceClient.GetObligationCalculationByOrganisationIdAsync(organisationId);
        }
    }
}
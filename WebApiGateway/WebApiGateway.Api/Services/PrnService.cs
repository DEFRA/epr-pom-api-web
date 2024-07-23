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

        public async Task<List<PrnModel>> GetAllPrnsForOrganisation(Guid organisationId)
        {
            return await _prnServiceClient.GetAllPrnsForOrganisation(organisationId);
        }

        public async Task<PrnModel> GetPrnById(int id)
        {
            return await _prnServiceClient.GetPrnById(id);
        }

        public async Task UpdatePrnStatusToAccepted(int id)
        {
            await _prnServiceClient.UpdatePrnStatusToAccepted(id);
        }
    }
}

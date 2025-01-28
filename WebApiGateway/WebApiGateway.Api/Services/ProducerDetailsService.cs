using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.ProducerDetails;

namespace WebApiGateway.Api.Services;

public class ProducerDetailsService(
    IProducerDetailsClient producerDetailsClient,
    ILogger<ProducerDetailsService> logger)
    : IProducerDetailsService
{
    public async Task<GetProducerDetailsResponse?> GetProducerDetails(int organisationNumber)
    {
        logger.LogDebug("Get Producer Details For Organisation Id {OrganisationId}", organisationNumber);
        return await producerDetailsClient.GetProducerDetails(organisationNumber);
    }
}
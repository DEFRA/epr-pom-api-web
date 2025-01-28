using WebApiGateway.Core.Models.ProducerDetails;

namespace WebApiGateway.Api.Clients.Interfaces;

public interface IProducerDetailsClient
{
    Task<GetProducerDetailsResponse?> GetProducerDetails(int organisationId);
}
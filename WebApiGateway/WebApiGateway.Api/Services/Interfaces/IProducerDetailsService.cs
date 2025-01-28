using WebApiGateway.Core.Models.ProducerDetails;

namespace WebApiGateway.Api.Services.Interfaces;

public interface IProducerDetailsService
{
    Task<GetProducerDetailsResponse?> GetProducerDetails(int organisationNumber);
}
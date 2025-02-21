using WebApiGateway.Core.Models.RegistrationFeeCalculation;

namespace WebApiGateway.Api.Clients.Interfaces;

public interface IRegistrationFeeCalculationDetailsClient
{
    Task<RegistrationFeeCalculationDetails[]?> GetRegistrationFeeCalculationDetails(Guid fileId);
}
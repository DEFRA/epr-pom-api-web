using WebApiGateway.Core.Models.RegistrationFeeCalculation;

namespace WebApiGateway.Api.Clients;

public interface IPaymentServiceClient
{
    Task<RegistrationFeeCalculationDetails[]?> GetRegistrationFeeCalculationDetails(Guid submissionId);
}
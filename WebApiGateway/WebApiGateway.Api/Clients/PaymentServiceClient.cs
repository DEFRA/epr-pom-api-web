using System.Net;
using Newtonsoft.Json;
using WebApiGateway.Core.Models.RegistrationFeeCalculation;

namespace WebApiGateway.Api.Clients;

public class PaymentServiceClient(
    HttpClient httpClient,
    ILogger<PaymentServiceClient> logger) : IPaymentServiceClient
{
    public async Task<RegistrationFeeCalculationDetails[]?> GetRegistrationFeeCalculationDetails(Guid submissionId)
    {
        var response =
            await httpClient.GetAsync($"v1/registration-submission-data/{submissionId}/fee-calculation-details");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogInformation("Registration fee calculation not found in payment service");
            return null;
        }
        
        // anything other than 200 or 404 is unexpected
        if (response.StatusCode != HttpStatusCode.OK)
        {
            response.EnsureSuccessStatusCode();
        }

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<RegistrationFeeCalculationDetails[]>(content);
    }
}
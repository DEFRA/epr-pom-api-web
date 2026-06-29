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
            // once we have proven the flow in prod, we should get rid of the try-catch and allow EnsureSuccessStatusCode to throw up the stack
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException exception)
            {
                logger.LogError(exception, "An error occurred retrieving fee calculation details for submission id {submissionId}", submissionId);
                return null;
            }
        }

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<RegistrationFeeCalculationDetails[]>(content);
    }
}
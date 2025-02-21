using System.Net;
using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Core.Models.RegistrationFeeCalculation;

namespace WebApiGateway.Api.Clients;

public class RegistrationFeeCalculationDetailsClient(
    HttpClient httpClient,
    ILogger<IRegistrationFeeCalculationDetailsClient> logger)
    : IRegistrationFeeCalculationDetailsClient
{
    public async Task<RegistrationFeeCalculationDetails[]?> GetRegistrationFeeCalculationDetails(Guid fileId)
    {
        var response = await httpClient.GetAsync($"registration-fee-calculation-details/get-registration-fee-calculation-details/{fileId}");

        // response.StatusCode == HttpStatusCode.BadRequest
        // response.StatusCode == HttpStatusCode.NoContent
        // response.StatusCode == HttpStatusCode.InternalServerError
        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError("Error Getting registration fee calculation details, StatusCode : {StatusCode} ({ReasonPhrase})", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<RegistrationFeeCalculationDetails[]>(content);
    }
}
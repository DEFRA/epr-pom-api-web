using System.Net;
using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Core.Models.ProducerDetails;

namespace WebApiGateway.Api.Clients;

public class ProducerDetailsClient(
    HttpClient httpClient,
    ILogger<ProducerDetailsClient> logger)
    : IProducerDetailsClient
{
    public async Task<GetProducerDetailsResponse?> GetProducerDetails(int organisationId)
    {
        var response = await httpClient.GetAsync($"producer-details/get-producer-details/{organisationId}");

        // response.StatusCode == HttpStatusCode.BadRequest
        // response.StatusCode == HttpStatusCode.NoContent
        // response.StatusCode == HttpStatusCode.InternalServerError
        if (response.StatusCode != HttpStatusCode.OK)
        {
            logger.LogError("Error Getting Producer Details, Response status code does not indicate success: {StatusCode} ({ReasonPhrase})", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<GetProducerDetailsResponse>(content);
    }
}
using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Clients
{
    public class PrnServiceClient : IPrnServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PrnServiceClient> _logger;

        public PrnServiceClient(HttpClient httpClient, ILogger<PrnServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<PrnModel>> GetAllPrnsForOrganisation(Guid organisationId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"organisation?orgId={organisationId}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<PrnModel>>(content);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "An error occurred retrieving prns for organisation {organisationId}", organisationId);
                throw;
            }
        }

        public async Task<PrnModel> GetPrnById(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{id}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<PrnModel>(content);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "An error occurred retrieving prns for Id {prnId}", id);
                throw;
            }
        }

        public async Task UpdatePrnStatusToAccepted(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{id}", null);

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "An error occurred updating prns status for Id {prnId}", id);
                throw;
            }
        }
    }
}

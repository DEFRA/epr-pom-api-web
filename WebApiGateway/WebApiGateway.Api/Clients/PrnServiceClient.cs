using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Clients
{
    public class PrnServiceClient : IPrnServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PrnServiceClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAccountServiceClient _accountServiceClient;

        public PrnServiceClient(
                    HttpClient httpClient,
                    ILogger<PrnServiceClient> logger,
                    IHttpContextAccessor httpContextAccessor,
                    IAccountServiceClient accountServiceClient)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<List<PrnModel>> GetAllPrnsForOrganisation()
        {
            try
            {
                await ConfigureHttpClientAsync();
                var response = await _httpClient.GetAsync("prn/organisation");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<PrnModel>>(content);
            }
            catch (HttpRequestException exception)
            {
                _httpClient.DefaultRequestHeaders.TryGetValues("X-EPR-ORGANISATION", out var orgId);
                _logger.LogError(exception, "An error occurred retrieving prns for organisation {organisationId}", orgId?.First() ?? "NoOrgId");
                throw;
            }
        }

        public async Task<PrnModel> GetPrnById(Guid id)
        {
            try
            {
                await ConfigureHttpClientAsync();
                var response = await _httpClient.GetAsync($"prn/{id}");

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

        public async Task UpdatePrnStatus(List<UpdatePrnStatus> updatePrns)
        {
            try
            {
                await ConfigureHttpClientAsync();
                var response = await _httpClient.PostAsJsonAsync($"prn/status", updatePrns);

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "An error occurred updating prns status");
                throw;
            }
        }

        public async Task<List<ObligationCalculation>> GetObligationCalculationByOrganisationIdAsync(int organisationId)
        {
            try
            {
                await ConfigureHttpClientAsync();
                var response = await _httpClient.GetAsync($"/prn/v1/obligationcalculation/{organisationId}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<ObligationCalculation>>(content);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, $"An error occurred retrievig obligation calculations for organisation id {organisationId}", organisationId);
                throw;
            }
        }

        private async Task ConfigureHttpClientAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.UserId();

            try
            {
                var userAccount = await _accountServiceClient.GetUserAccount(userId);

                _httpClient.DefaultRequestHeaders.AddIfNotExists("X-EPR-ORGANISATION", userAccount.User.Organisations[0].Id.ToString());
                _httpClient.DefaultRequestHeaders.AddIfNotExists("X-EPR-USER", userId.ToString());
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "Error getting user accounts with id {userId}", userId);
                throw;
            }
        }
    }
}

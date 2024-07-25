using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Core.Models.Prns;
using WebApiGateway.Core.Models.UserAccount;

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

        public async Task<List<PrnModel>> GetAllPrnsForOrganisation(Guid organisationId)
        {
            try
            {
                ConfigureHttpClientAsync();
                var response = await _httpClient.GetAsync($"prn/organisation?orgId={organisationId}");

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

        public async Task<PrnModel> GetPrnById(Guid id)
        {
            try
            {
                ConfigureHttpClientAsync();
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

        public async Task UpdatePrnStatusToAccepted(Guid id)
        {
            try
            {
                ConfigureHttpClientAsync();
                var response = await _httpClient.PatchAsync($"prn/status/{id}", null);

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, "An error occurred updating prns status for Id {prnId}", id);
                throw;
            }
        }

        private async Task ConfigureHttpClientAsync()
        {
            Guid userId = Guid.NewGuid();
            UserAccount userAccount;
            OrganisationDetail organisation;

            try
            {
                userId = _httpContextAccessor.HttpContext.User.UserId();
                userAccount = await _accountServiceClient.GetUserAccount(userId);
                organisation = userAccount.User.Organisations.First();

                _httpClient.DefaultRequestHeaders.AddIfNotExists("OrganisationId", organisation.Id.ToString());
                _httpClient.DefaultRequestHeaders.AddIfNotExists("UserId", userId.ToString());
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(exception, $"Error getting user accounts with id {userId}");
                throw;
            }
        }
    }
}

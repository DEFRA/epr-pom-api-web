namespace WebApiGateway.Api.Clients;

using Core.Models.UserAccount;
using Interfaces;
using Newtonsoft.Json;

public class AccountServiceClient : IAccountServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountServiceClient> _logger;

    public AccountServiceClient(HttpClient httpClient, ILogger<AccountServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserAccount> GetUserAccount(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"users/user-organisations?userId={userId}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<UserAccount>(content);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "An error occurred retrieving user by id: {userId}", userId);
            throw;
        }
    }
}
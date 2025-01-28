using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Core.Models.UserAccount;

namespace WebApiGateway.Api.Clients;

public class AccountServiceClient(
    HttpClient httpClient,
    ILogger<AccountServiceClient> logger)
    : IAccountServiceClient
{
    public async Task<UserAccount> GetUserAccount(Guid userId)
    {
        try
        {
            var response = await httpClient.GetAsync($"users/user-organisations?userId={userId}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<UserAccount>(content);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "An error occurred retrieving user by id: {userId}", userId);
            throw;
        }
    }
}
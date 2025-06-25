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
        string endpoint = $"users/user-organisations?userId={userId}";
        return await GetUserAccount(httpClient, logger, userId, endpoint);
    }

    public async Task<UserAccount> GetUserAccountIncludeDeleted(Guid userId)
    {
        string endpoint = $"users/user-organisations-include-deleted?userId={userId}";
        return await GetUserAccount(httpClient, logger, userId, endpoint);
    }

    private static async Task<UserAccount> GetUserAccount(HttpClient httpClient, ILogger<AccountServiceClient> logger, Guid userId, string endpoint)
    {
        try
        {
            var response = await httpClient.GetAsync(endpoint);

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
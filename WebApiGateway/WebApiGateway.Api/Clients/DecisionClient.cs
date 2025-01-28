using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Core.Models.Decision;

namespace WebApiGateway.Api.Clients;

public class DecisionClient(
    HttpClient httpClient,
    IAccountServiceClient accountServiceClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<DecisionClient> logger)
    : IDecisionClient
{
    public async Task<RegulatorDecision> GetDecisionAsync(string queryString)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.GetAsync($"decisions{queryString}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var decisions = JsonConvert.DeserializeObject<List<RegulatorDecision>>(content).Where(s => s.Created != null);

            var lastDecision = decisions.OrderBy(o => o.Created).LastOrDefault();

            return lastDecision ?? new RegulatorDecision();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error getting Regulator Decisions");
            throw;
        }
    }

    private async Task ConfigureHttpClientAsync()
    {
        var userId = httpContextAccessor.HttpContext.User.UserId();
        var userAccount = await accountServiceClient.GetUserAccount(userId);
        var organisation = userAccount.User.Organisations[0];
        httpClient.DefaultRequestHeaders.AddIfNotExists("OrganisationId", organisation.Id.ToString());
        httpClient.DefaultRequestHeaders.AddIfNotExists("UserId", userId.ToString());
    }
}
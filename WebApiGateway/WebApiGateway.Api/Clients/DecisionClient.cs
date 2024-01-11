﻿namespace WebApiGateway.Api.Clients;

using Core.Models.Decision;
using Interfaces;
using Newtonsoft.Json;
using WebApiGateway.Api.Extensions;

public class DecisionClient : IDecisionClient
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DecisionClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAccountServiceClient _accountServiceClient;

    public DecisionClient(
        HttpClient httpClient,
        IAccountServiceClient accountServiceClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<DecisionClient> logger)
    {
        _httpClient = httpClient;
        _accountServiceClient = accountServiceClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<PomDecision> GetDecisionAsync(string queryString)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await _httpClient.GetAsync($"decisions{queryString}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var decisions = JsonConvert.DeserializeObject<List<PomDecision>>(content).Where(s => s.Created != null);

            var lastDecision = decisions.OrderBy(o => o.Created).LastOrDefault();

            if (lastDecision == null)
            {
                return new PomDecision();
            }

            return lastDecision;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error getting Regulator Decisions");
            throw;
        }
    }

    private async Task ConfigureHttpClientAsync()
    {
        var userId = _httpContextAccessor.HttpContext.User.UserId();
        var userAccount = await _accountServiceClient.GetUserAccount(userId);
        var organisation = userAccount.User.Organisations.First();

        _httpClient.DefaultRequestHeaders.AddIfNotExists("OrganisationId", organisation.Id.ToString());
        _httpClient.DefaultRequestHeaders.AddIfNotExists("UserId", userId.ToString());
    }
}
namespace WebApiGateway.Api.Services;

using Clients.Interfaces;
using Core.Models.Decision;
using Interfaces;

public class DecisionService : IDecisionService
{
    private readonly IDecisionClient _decisionClient;

    public DecisionService(IDecisionClient decisionClient)
    {
        _decisionClient = decisionClient;
    }

    public async Task<PomDecision> GetDecisionAsync(string queryString)
    {
        return await _decisionClient.GetDecisionAsync(queryString);
    }
}
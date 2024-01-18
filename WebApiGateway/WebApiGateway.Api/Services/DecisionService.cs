using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Decision;

namespace WebApiGateway.Api.Services;

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
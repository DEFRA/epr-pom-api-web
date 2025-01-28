using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Decision;

namespace WebApiGateway.Api.Services;

public class DecisionService(IDecisionClient decisionClient) : IDecisionService
{
    public async Task<RegulatorDecision> GetDecisionAsync(string queryString)
    {
        return await decisionClient.GetDecisionAsync(queryString);
    }
}
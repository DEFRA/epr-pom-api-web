using WebApiGateway.Core.Models.Decision;

namespace WebApiGateway.Api.Clients.Interfaces;

public interface IDecisionClient
{
    Task<PomDecision> GetDecisionAsync(string queryString);
}
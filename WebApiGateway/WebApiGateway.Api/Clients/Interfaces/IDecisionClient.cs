namespace WebApiGateway.Api.Clients.Interfaces;

using Core.Models.Decision;

public interface IDecisionClient
{
    Task<PomDecision> GetDecisionAsync(string queryString);
}
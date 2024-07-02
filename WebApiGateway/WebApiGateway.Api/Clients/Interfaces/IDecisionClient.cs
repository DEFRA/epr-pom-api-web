using WebApiGateway.Core.Models.Decision;

namespace WebApiGateway.Api.Clients.Interfaces;

public interface IDecisionClient
{
    Task<RegulatorDecision> GetDecisionAsync(string queryString);
}
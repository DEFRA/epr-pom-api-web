using WebApiGateway.Core.Models.Decision;

namespace WebApiGateway.Api.Services.Interfaces;

public interface IDecisionService
{
    Task<PomDecision> GetDecisionAsync(string queryString);
}
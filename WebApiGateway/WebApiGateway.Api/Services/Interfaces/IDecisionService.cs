namespace WebApiGateway.Api.Services.Interfaces;

using Core.Models.Decision;

public interface IDecisionService
{
    Task<PomDecision> GetDecisionAsync(string queryString);
}
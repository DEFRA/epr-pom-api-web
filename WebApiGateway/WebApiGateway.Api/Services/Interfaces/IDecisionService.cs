using WebApiGateway.Core.Models.Decision;

namespace WebApiGateway.Api.Services.Interfaces;

public interface IDecisionService
{
    Task<RegulatorDecision> GetDecisionAsync(string queryString);
}
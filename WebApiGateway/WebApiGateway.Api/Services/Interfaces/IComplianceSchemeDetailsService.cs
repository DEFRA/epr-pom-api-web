namespace WebApiGateway.Api.Services.Interfaces;

public interface IComplianceSchemeDetailsService
{
    Task<Guid?> GetComplianceSchemeIdAsync();
}
namespace WebApiGateway.Api.Clients.Interfaces;

public interface IWasteObligationsProxy
{
    Task<string?> GetComplianceDeclarations(int obligationYear, CancellationToken cancellationToken);
}
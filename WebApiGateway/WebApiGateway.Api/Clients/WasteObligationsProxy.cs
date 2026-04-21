using WebApiGateway.Api.Clients.Interfaces;

namespace WebApiGateway.Api.Clients;

public class WasteObligationsProxy(HttpClient httpClient) : IWasteObligationsProxy
{
    public async Task<string?> GetComplianceDeclarations(int? obligationYear, CancellationToken cancellationToken)
    {
        // See WasteObligationsAuthorisationHandler for :organisationId replacement 
        return await Get(
            $"/organisations/:organisationId/compliance-declarations?obligationYear={obligationYear}",
            cancellationToken);
    }

    private async Task<string?> Get(string requestUri, CancellationToken cancellationToken) =>
        await httpClient.GetStringAsync(requestUri, cancellationToken);
}
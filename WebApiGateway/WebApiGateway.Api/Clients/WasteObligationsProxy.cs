using WebApiGateway.Api.Clients.Interfaces;

namespace WebApiGateway.Api.Clients;

public class WasteObligationsProxy(HttpClient httpClient) : IWasteObligationsProxy
{
    public async Task<string?> Get(string requestUri, CancellationToken cancellationToken)
    {
        return await httpClient.GetStringAsync(requestUri, cancellationToken);
    }
}
namespace WebApiGateway.Api.Clients.Interfaces;

public interface IWasteObligationsProxy
{
    Task<string?> Get(string requestUri, CancellationToken cancellationToken);
}
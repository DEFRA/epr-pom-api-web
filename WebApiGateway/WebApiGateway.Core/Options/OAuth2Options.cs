namespace WebApiGateway.Core.Options;

public class OAuth2Options
{
    public string TokenEndpoint { get; init; }

    public string ClientId { get; init; }

    public string ClientSecret { get; init; }

    public string? Scope { get; init; }
}
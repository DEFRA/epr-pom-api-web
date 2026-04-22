using System.Text.Json.Serialization;

namespace WebApiGateway.Api.Handlers;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }
}
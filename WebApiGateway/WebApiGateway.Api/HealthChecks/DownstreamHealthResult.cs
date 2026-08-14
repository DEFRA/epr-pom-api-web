using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace WebApiGateway.Api.HealthChecks;

public sealed record DownstreamHealthResult(
    string Status,
    string Endpoint,
    int? StatusCode,
    long DurationMs,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] JsonNode? Response = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Failure = null);

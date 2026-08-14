using System.Text.Json.Serialization;

namespace WebApiGateway.Api.HealthChecks;

public sealed record AggregateHealthReport(
    string Status,
    IReadOnlyDictionary<string, DownstreamHealthResult> Results,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] bool DeepLimited = false);

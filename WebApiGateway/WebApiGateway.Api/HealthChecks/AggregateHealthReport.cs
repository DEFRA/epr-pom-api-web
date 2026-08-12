using System.Text.Json.Nodes;

namespace WebApiGateway.Api.HealthChecks;

public sealed record AggregateHealthReport(string Status, IReadOnlyDictionary<string, DownstreamHealthResult> Results);

namespace WebApiGateway.Api.HealthChecks;

public class AggregateHealthOptions
{
    public const string SectionName = "Health:All";

    public int DownstreamTimeoutSeconds { get; set; } = 10;

    public int MaximumResponseBodyBytes { get; set; } = 65_536;
}

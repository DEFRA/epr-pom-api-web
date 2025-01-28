using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Options;

[ExcludeFromCodeCoverage]
public class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = null!;

    public int? TimeToLiveInMinutes { get; set; } = null;
}

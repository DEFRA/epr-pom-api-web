using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Options;

[ExcludeFromCodeCoverage]
public class RedisConfig
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = null!;
}

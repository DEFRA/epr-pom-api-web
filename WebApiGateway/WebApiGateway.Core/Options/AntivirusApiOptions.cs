using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Options;

[ExcludeFromCodeCoverage]
public class AntivirusApiOptions
{
    public const string Section = "AntivirusApi";

    public string BaseUrl { get; set; } = string.Empty;

    public string SubscriptionKey { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public int Timeout { get; set; }

    public bool EnableDirectAccess { get; set; } = false;

    public string CollectionSuffix { get; set; } = string.Empty;
}
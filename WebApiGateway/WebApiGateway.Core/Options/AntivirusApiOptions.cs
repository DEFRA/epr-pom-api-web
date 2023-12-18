namespace WebApiGateway.Core.Options;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class AntivirusApiOptions
{
    public const string Section = "AntivirusApi";

    public string BaseUrl { get; set; }

    public string SubscriptionKey { get; set; }

    public string TenantId { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public string Scope { get; set; }

    public int Timeout { get; set; }

    public bool EnableDirectAccess { get; set; } = false;

    public string CollectionSuffix { get; set; }
}
using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Options;

[ExcludeFromCodeCoverage]
public class PrnServiceApiOptions
{
    public const string Section = "PrnServiceApi";

    public string BaseUrl { get; set; } = string.Empty;

    public int Timeout { get; set; }

    public string ClientId { get; set; } = string.Empty;
}
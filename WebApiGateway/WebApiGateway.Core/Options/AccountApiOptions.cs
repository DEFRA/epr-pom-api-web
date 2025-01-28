using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Options;

[ExcludeFromCodeCoverage]
public class AccountApiOptions
{
    public const string Section = "AccountApi";

    public string BaseUrl { get; set; } = string.Empty;

    public int Timeout { get; set; }

    public string ClientId { get; set; } = string.Empty;
}
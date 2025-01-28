using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Options;

[ExcludeFromCodeCoverage]
public class CommonDataApiOptions
{
    public const string Section = "CommonDataApi";

    public string BaseUrl { get; set; } = string.Empty;

    public int Timeout { get; set; }
}

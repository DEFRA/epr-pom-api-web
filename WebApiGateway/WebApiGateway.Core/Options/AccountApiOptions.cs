namespace WebApiGateway.Core.Options;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class AccountApiOptions
{
    public const string Section = "AccountApi";

    public string BaseUrl { get; set; }

    public int Timeout { get; set; }

    public string ClientId { get; set; }
}
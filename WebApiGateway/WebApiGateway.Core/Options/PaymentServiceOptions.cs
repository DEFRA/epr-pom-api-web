using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Options;

[ExcludeFromCodeCoverage]
public class PaymentServiceOptions
{
    public const string Section = "PaymentService";

    public string BaseUrl { get; set; } = string.Empty;

    public string ClientId { get; set; }
}

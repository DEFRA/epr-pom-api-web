using System.Net;

namespace WebApiGateway.Core.Options;

public class WasteObligationsOptions : OAuth2Options
{
    public const string SectionName = "WasteObligations";

    public string BaseAddress { get; init; }

    public void Configure(HttpClient httpClient)
    {
        httpClient.BaseAddress = new Uri(BaseAddress);

        if (httpClient.BaseAddress.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
        {
            httpClient.DefaultRequestVersion = HttpVersion.Version20;
        }
    }
}
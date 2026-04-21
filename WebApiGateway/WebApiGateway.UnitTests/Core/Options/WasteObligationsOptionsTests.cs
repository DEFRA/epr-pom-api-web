using System.Net;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiGateway.Core.Options;

namespace WebApiGateway.UnitTests.Core.Options;

[TestClass]
public class WasteObligationsOptionsTests
{
    [DataTestMethod]
    [DataRow("http", 1)]
    [DataRow("https", 2)]
    public void WhenScheme_ShouldBeExpectedVersion(string scheme, int expectedVersion)
    {
        var subject = new WasteObligationsOptions
        {
            BaseAddress = $"{scheme}://waste-obligations",
            TokenEndpoint = "http://oauth2/token",
            ClientId = "client_id",
            ClientSecret = "client_secret",
        };

        var client = new HttpClient();
        subject.Configure(client);

        switch (expectedVersion)
        {
            case 1:
                client.DefaultRequestVersion.Should().Be(HttpVersion.Version11);
                break;
            case 2:
                client.DefaultRequestVersion.Should().Be(HttpVersion.Version20);
                break;
        }
    }
}
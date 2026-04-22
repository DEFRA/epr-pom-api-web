using System.Net;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace WebApiGateway.UnitTests.Support.Extensions;

public static class WasteObligationsExtensions
{
    public static void StubWasteObligationsComplianceDeclarationsRequest(
        this WireMockServer wireMock,
        Guid organisationId,
        int obligationYear = 2026,
        string? accessToken = null)
    {
        var request = Request.Create().UsingGet()
            .WithPath($"/organisations/{organisationId:D}/compliance-declarations")
            .WithParam("obligationYear", obligationYear.ToString());

        if (accessToken is not null)
        {
            request = request.WithHeader("Authorization", $"Bearer {accessToken}");
        }

        wireMock
            .Given(request)
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBodyAsJson(new
                    {
                    }));
    }
}
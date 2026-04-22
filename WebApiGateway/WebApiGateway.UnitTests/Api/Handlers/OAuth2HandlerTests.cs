using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiGateway.Api.Handlers;
using WebApiGateway.Core.Options;
using WebApiGateway.UnitTests.Support;
using WebApiGateway.UnitTests.Support.Extensions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace WebApiGateway.UnitTests.Api.Handlers;

[TestClass]
public class OAuth2HandlerTests
{
    private WireMockServer WireMock { get; set; }
    private WireMockContext Context { get; set; }
    
    [TestInitialize]
    public void TestInitialize()
    {
        var context = new WireMockContext();
        
        WireMock = context.Server;
        WireMock.Reset();
        Context = context;
    }
    
    [DataTestMethod]
    [DataRow(1)]
    [DataRow(2)]
    public async Task WhenRequestsAreMade_ShouldGetTokenOnce(int requests)
    {
        const string AccessToken = nameof(AccessToken);
        WireMock.StubTokenRequest(AccessToken);
        WireMock
            .Given(
                Request.Create().UsingGet().WithPath("/endpoint").WithHeader("Authorization", $"Bearer {AccessToken}"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));

        var services = CreateServices();
        await using var sp = services.BuildServiceProvider();

        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(OAuth2HandlerTests));

        for (var i = 0; i < requests; i++)
        {
            var response = await client.GetAsync("/endpoint", CancellationToken.None);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        WireMock.LogEntries.Count(x => x.RequestMessage?.Path == "/token").Should().Be(1);
        WireMock.LogEntries.Count(x => x.RequestMessage?.Path == "/endpoint").Should().Be(requests);
    }

    [TestMethod]
    public async Task WhenConcurrentRequestsAreMade_ShouldGetTokenOnce()
    {
        const string AccessToken = nameof(AccessToken);
        WireMock.StubTokenRequest(AccessToken);
        WireMock
            .Given(
                Request.Create().UsingGet().WithPath("/endpoint").WithHeader("Authorization", $"Bearer {AccessToken}"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));

        var services = CreateServices();
        await using var sp = services.BuildServiceProvider();

        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(OAuth2HandlerTests));

        var request1 = client.GetAsync("/endpoint", CancellationToken.None);
        var request2 = client.GetAsync("/endpoint", CancellationToken.None);

        await Task.WhenAll(request1, request2);

        WireMock.LogEntries.Count(x => x.RequestMessage?.Path == "/token").Should().Be(1);
        WireMock.LogEntries.Count(x => x.RequestMessage?.Path == "/endpoint").Should().Be(2);
    }

    [TestMethod]
    public async Task WhenTokenExpires_ShouldGetTokenAgain()
    {
        const string AccessToken = nameof(AccessToken);
        WireMock.StubTokenRequest(AccessToken, expiryInSeconds: 60);
        WireMock
            .Given(
                Request.Create().UsingGet().WithPath("/endpoint").WithHeader("Authorization", $"Bearer {AccessToken}"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));

        var services = CreateServices();
        await using var sp = services.BuildServiceProvider();

        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(OAuth2HandlerTests));

        await client.GetAsync("/endpoint", CancellationToken.None);
        await client.GetAsync("/endpoint", CancellationToken.None);

        WireMock.LogEntries.Count(x => x.RequestMessage?.Path == "/token").Should().Be(2);
        WireMock.LogEntries.Count(x => x.RequestMessage?.Path == "/endpoint").Should().Be(2);
    }

    [TestMethod]
    public async Task WhenNoScope_ShouldNotSendScope()
    {
        const string AccessToken = nameof(AccessToken);
        WireMock.StubTokenRequest(AccessToken, expiryInSeconds: 60, scope: null);
        WireMock
            .Given(
                Request.Create().UsingGet().WithPath("/endpoint").WithHeader("Authorization", $"Bearer {AccessToken}"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));

        var services = CreateServices(scope: null);
        await using var sp = services.BuildServiceProvider();

        var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(OAuth2HandlerTests));

        await client.GetAsync("/endpoint", CancellationToken.None);

        WireMock
            .LogEntries.Count(x =>
                x.RequestMessage?.Path == "/token" && x.RequestMessage?.Body?.Contains("scope") is false)
            .Should()
            .Be(1);
        WireMock.LogEntries.Count(x => x.RequestMessage?.Path == "/endpoint").Should().Be(1);
    }

    private ServiceCollection CreateServices(string? scope = "scope")
    {
        var result = new ServiceCollection();

        result
            .AddHttpClient(nameof(OAuth2HandlerTests))
            .AddHttpMessageHandler(sp => new OAuth2Handler(
                new OAuth2TokenCache(
                    sp.GetRequiredService<IHttpClientFactory>(),
                    new OAuth2Options
                    {
                        TokenEndpoint = Context.BaseAddress + "/token",
                        ClientId = "client_id",
                        ClientSecret = "client_secret",
                        Scope = scope,
                    })))
            .ConfigureHttpClient(httpClient => httpClient.BaseAddress = new Uri(Context.BaseAddress));

        return result;
    }
}
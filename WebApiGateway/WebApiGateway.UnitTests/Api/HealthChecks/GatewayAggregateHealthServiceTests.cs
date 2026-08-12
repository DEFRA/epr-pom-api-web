using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiGateway.Api.HealthChecks;
using WebApiGateway.Core.Options;

namespace WebApiGateway.UnitTests.Api.HealthChecks;

[UsesVerify]
[TestClass]
public partial class GatewayAggregateHealthServiceTests
{
    [TestMethod]
    public async Task CheckAsync_WhenNotDeep_CallsShallowHealthEndpoints()
    {
        var handler = new RecordingHandler();
        var service = CreateService(handler);

        var report = await service.CheckAsync(false, 0, CancellationToken.None);

        report.Status.Should().Be("Healthy");
        handler.RequestUris.Should().BeEquivalentTo(
            [
                "https://account.test/account/admin/health",
                "https://submission.test/submission/admin/health",
                "https://payment.test/payment/admin/health",
                "https://prn.test/prn/admin/health",
                "https://common.test/common/admin/health",
                "https://waste.test/waste/health",
            ]);
        handler.ClientNames.Should().BeEquivalentTo(
            [
                DownstreamHealthClientNames.AccountApi,
                DownstreamHealthClientNames.SubmissionStatusApi,
                DownstreamHealthClientNames.PaymentService,
                DownstreamHealthClientNames.PrnServiceApi,
                DownstreamHealthClientNames.CommonDataApi,
                DownstreamHealthClientNames.WasteObligations,
            ]);
        report.Results["WasteObligations"].Response.Should().BeNull();
    }

    [TestMethod]
    public async Task CheckAsync_WhenDeep_CallsWasteObligationsExtendedHealthOnly()
    {
        var handler = new RecordingHandler();
        var service = CreateService(handler);

        var report = await service.CheckAsync(true, 0, CancellationToken.None);

        handler.RequestUris.Should().Contain("https://waste.test/waste/health/all");
        handler.RequestUris.Should().NotContain("https://waste.test/waste/health");
        handler.RequestUris.Should().Contain("https://account.test/account/admin/health");
        handler.RequestUris.Should().Contain("https://submission.test/submission/admin/health");
        handler.Requests.Single(request => request.Uri.EndsWith("/health/all", StringComparison.Ordinal)).Hop.Should().Be("1");
        await VerifyJson(JsonSerializer.Serialize(report, new JsonSerializerOptions(JsonSerializerDefaults.Web)))
            .UseStrictJson()
            .ScrubMember("durationMs");
    }

    [TestMethod]
    public async Task CheckAsync_WhenDeep_AddsTheNextHopOnlyToWasteObligations()
    {
        var handler = new RecordingHandler();
        var service = CreateService(handler);

        await service.CheckAsync(true, 1, CancellationToken.None);

        handler.Requests.Single(request => request.Uri.EndsWith("/health/all", StringComparison.Ordinal)).Hop.Should().Be("2");
        handler.Requests.Where(request => !request.Uri.EndsWith("/health/all", StringComparison.Ordinal)).Should().AllSatisfy(request => request.Hop.Should().BeNull());
    }

    [TestMethod]
    public async Task CheckAsync_WhenMaximumHopReached_UsesWasteObligationsShallowHealth()
    {
        var handler = new RecordingHandler();
        var service = CreateService(handler);

        var report = await service.CheckAsync(true, 2, CancellationToken.None);

        report.DeepLimited.Should().BeTrue();
        handler.RequestUris.Should().Contain("https://waste.test/waste/health");
        handler.RequestUris.Should().NotContain("https://waste.test/waste/health/all");
        handler.Requests.Single(request => request.Uri == "https://waste.test/waste/health").Hop.Should().BeNull();
        report.Results["WasteObligations"].Response.Should().BeNull();
    }

    private static GatewayAggregateHealthService CreateService(RecordingHandler handler) => new(
        new TestHttpClientFactory(handler),
        Options.Create(new AccountApiOptions { BaseUrl = "https://account.test/account" }),
        Options.Create(new SubmissionStatusApiOptions { BaseUrl = "https://submission.test/submission" }),
        Options.Create(new PaymentServiceOptions { BaseUrl = "https://payment.test/payment" }),
        Options.Create(new PrnServiceApiOptions { BaseUrl = "https://prn.test/prn" }),
        Options.Create(new CommonDataApiOptions { BaseUrl = "https://common.test/common" }),
        Options.Create(new WasteObligationsOptions { BaseAddress = "https://waste.test/waste/" }),
        Options.Create(new AggregateHealthOptions()));

    private sealed class TestHttpClientFactory(RecordingHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            handler.ClientNames.Add(name);
            return new HttpClient(handler, disposeHandler: false);
        }
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public ConcurrentBag<string> RequestUris { get; } = [];

        public ConcurrentBag<string> ClientNames { get; } = [];

        public ConcurrentBag<RecordedRequest> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUris.Add(request.RequestUri!.ToString());
            Requests.Add(new RecordedRequest(
                request.RequestUri.ToString(),
                request.Headers.TryGetValues(AggregateHealthHop.HeaderName, out var values) ? values.Single() : null));
            var body = request.RequestUri.AbsolutePath.EndsWith("/health/all", StringComparison.Ordinal)
                ? "{\"status\":\"Healthy\",\"results\":{}}"
                : "Healthy";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }

        public sealed record RecordedRequest(string Uri, string? Hop);
    }
}

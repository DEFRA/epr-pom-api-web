using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Identity;
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
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

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
        await VerifyJson(JsonSerializer.Serialize(report, SerializerOptions))
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

    [TestMethod]
    public async Task CheckAsync_WhenDownstreamIsUnauthorised_ReturnsAnAuthenticationFailure()
    {
        var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)));
        var service = CreateService(handler);

        var report = await service.CheckAsync(false, 0, CancellationToken.None);

        report.Status.Should().Be("Unhealthy");
        report.Results.Values.Should().AllSatisfy(result =>
        {
            result.Status.Should().Be("Unhealthy");
            result.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
            result.Failure.Should().Be("authentication");
        });
    }

    [TestMethod]
    public async Task CheckAsync_WhenDeepResponseIsInvalid_ReturnsAnInvalidResponseFailure()
    {
        var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not json", Encoding.UTF8, "application/json"),
        }));
        var service = CreateService(handler);

        var report = await service.CheckAsync(true, 0, CancellationToken.None);

        report.Status.Should().Be("Unhealthy");
        report.Results["WasteObligations"].Failure.Should().Be("invalid_response");
    }

    [TestMethod]
    public async Task CheckAsync_WhenDeepResponseExceedsTheConfiguredLimit_ReturnsAnInvalidResponseFailure()
    {
        var handler = new RecordingHandler();
        var service = CreateService(handler, new AggregateHealthOptions { MaximumResponseBodyBytes = 1 });

        var report = await service.CheckAsync(true, 0, CancellationToken.None);

        report.Status.Should().Be("Unhealthy");
        report.Results["WasteObligations"].Failure.Should().Be("invalid_response");
    }

    [TestMethod]
    public async Task CheckAsync_WhenDeepResponseWithUnknownLengthExceedsTheConfiguredLimit_ReturnsAnInvalidResponseFailure()
    {
        var handler = new RecordingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new UnknownLengthContent("{}"),
        }));
        var service = CreateService(handler, new AggregateHealthOptions { MaximumResponseBodyBytes = 1 });

        var report = await service.CheckAsync(true, 0, CancellationToken.None);

        report.Status.Should().Be("Unhealthy");
        report.Results["WasteObligations"].Failure.Should().Be("invalid_response");
    }

    [TestMethod]
    public async Task CheckAsync_WhenDownstreamIsUnavailable_ReturnsAnUnavailableFailure()
    {
        var handler = new RecordingHandler((_, _) => Task.FromException<HttpResponseMessage>(new HttpRequestException()));
        var service = CreateService(handler);

        var report = await service.CheckAsync(false, 0, CancellationToken.None);

        report.Results.Values.Should().OnlyContain(result => result.Failure == "unavailable");
    }

    [TestMethod]
    public async Task CheckAsync_WhenDownstreamTimesOut_ReturnsATimeoutFailure()
    {
        var handler = new RecordingHandler((_, _) => Task.FromException<HttpResponseMessage>(new OperationCanceledException()));
        var service = CreateService(handler);

        var report = await service.CheckAsync(false, 0, CancellationToken.None);

        report.Results.Values.Should().OnlyContain(result => result.Failure == "timeout");
    }

    [TestMethod]
    public async Task CheckAsync_WhenDownstreamAuthenticationFails_ReturnsAnAuthenticationFailure()
    {
        var handler = new RecordingHandler((_, _) => Task.FromException<HttpResponseMessage>(new AuthenticationFailedException("Test failure")));
        var service = CreateService(handler);

        var report = await service.CheckAsync(false, 0, CancellationToken.None);

        report.Results.Values.Should().OnlyContain(result => result.Failure == "authentication");
    }

    [TestMethod]
    public async Task CheckAsync_WhenEndpointsAreInvalid_ReturnsConfigurationFailures()
    {
        var handler = new RecordingHandler();
        var endpoints = new GatewayAggregateHealthEndpoints(
            Options.Create(new AccountApiOptions { BaseUrl = "not-a-uri" }),
            Options.Create(new SubmissionStatusApiOptions { BaseUrl = null! }),
            Options.Create(new PaymentServiceOptions { BaseUrl = "https://payment.test/payment" }),
            Options.Create(new PrnServiceApiOptions { BaseUrl = "https://prn.test/prn" }),
            Options.Create(new CommonDataApiOptions { BaseUrl = "https://common.test/common" }),
            Options.Create(new WasteObligationsOptions { BaseAddress = "https://waste.test/waste/" }));
        var service = new GatewayAggregateHealthService(
            new TestHttpClientFactory(handler),
            endpoints,
            Options.Create(new AggregateHealthOptions()));

        var report = await service.CheckAsync(false, 0, CancellationToken.None);

        report.Results["AccountApi"].Failure.Should().Be("configuration");
        report.Results["SubmissionStatusApi"].Failure.Should().Be("configuration");
    }

    [TestMethod]
    public async Task CheckAsync_WhenTheCallerCancels_PropagatesTheCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var handler = new RecordingHandler((_, token) => Task.FromCanceled<HttpResponseMessage>(token));
        var service = CreateService(handler);

        Func<Task> action = () => service.CheckAsync(false, 0, cancellation.Token);

        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    private static GatewayAggregateHealthService CreateService(RecordingHandler handler, AggregateHealthOptions? healthOptions = null) => new(
        new TestHttpClientFactory(handler),
        new GatewayAggregateHealthEndpoints(
            Options.Create(new AccountApiOptions { BaseUrl = "https://account.test/account" }),
            Options.Create(new SubmissionStatusApiOptions { BaseUrl = "https://submission.test/submission" }),
            Options.Create(new PaymentServiceOptions { BaseUrl = "https://payment.test/payment" }),
            Options.Create(new PrnServiceApiOptions { BaseUrl = "https://prn.test/prn" }),
            Options.Create(new CommonDataApiOptions { BaseUrl = "https://common.test/common" }),
            Options.Create(new WasteObligationsOptions { BaseAddress = "https://waste.test/waste/" })),
        Options.Create(healthOptions ?? new AggregateHealthOptions()));

    private sealed class UnknownLengthContent(string content) : HttpContent
    {
        private readonly byte[] _content = Encoding.UTF8.GetBytes(content);

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => stream.WriteAsync(_content).AsTask();

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    private sealed class TestHttpClientFactory(RecordingHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            handler.ClientNames.Add(name);
            return new HttpClient(handler, disposeHandler: false);
        }
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? responseFactory = null) : HttpMessageHandler
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

            return responseFactory?.Invoke(request, cancellationToken) ?? Task.FromResult(CreateDefaultResponse(request));
        }

        private static HttpResponseMessage CreateDefaultResponse(HttpRequestMessage request)
        {
            var body = request.RequestUri!.AbsolutePath.EndsWith("/health/all", StringComparison.Ordinal)
                ? "{\"status\":\"Healthy\",\"results\":{}}"
                : "Healthy";

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
        }

        public sealed record RecordedRequest(string Uri, string? Hop);
    }
}

using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace WebApiGateway.Api.HealthChecks;

public sealed class GatewayAggregateHealthService(
    IHttpClientFactory httpClientFactory,
    GatewayAggregateHealthEndpoints endpoints,
    IOptions<AggregateHealthOptions> aggregateHealthOptions)
{
    private const string Healthy = "Healthy";
    private const string Unhealthy = "Unhealthy";

    public async Task<AggregateHealthReport> CheckAsync(bool deep, int hop, CancellationToken cancellationToken)
    {
        var effectiveDeep = deep && hop < aggregateHealthOptions.Value.MaximumDeepHealthHops;
        var checks = new[]
        {
            CheckAsync("AccountApi", DownstreamHealthClientNames.AccountApi, () => AdminHealth(endpoints.AccountApiBaseUrl, "api/"), false, null, cancellationToken),
            CheckAsync("SubmissionStatusApi", DownstreamHealthClientNames.SubmissionStatusApi, () => AdminHealth(endpoints.SubmissionStatusApiBaseUrl, "v1/"), false, null, cancellationToken),
            CheckAsync("PaymentService", DownstreamHealthClientNames.PaymentService, () => AdminHealth(endpoints.PaymentServiceBaseUrl, "api/"), false, null, cancellationToken),
            CheckAsync("PrnServiceApi", DownstreamHealthClientNames.PrnServiceApi, () => AdminHealth(endpoints.PrnServiceApiBaseUrl, "api/"), false, null, cancellationToken),
            CheckAsync("CommonDataApi", DownstreamHealthClientNames.CommonDataApi, () => AdminHealth(endpoints.CommonDataApiBaseUrl, "api/"), false, null, cancellationToken),
            CheckAsync("WasteObligations", DownstreamHealthClientNames.WasteObligations, () => WasteObligationsHealth(endpoints.WasteObligationsBaseAddress, effectiveDeep), effectiveDeep, effectiveDeep ? hop : null, cancellationToken),
        };

        var results = await Task.WhenAll(checks);
        var resultMap = results.ToDictionary(result => result.Name, result => result.Result, StringComparer.Ordinal);
        var status = resultMap.Values.All(result => result.Status == Healthy) ? Healthy : Unhealthy;

        return new AggregateHealthReport(status, resultMap, deep && !effectiveDeep);
    }

    private static Uri AdminHealth(string baseUrl, string apiPath) =>
        new(new Uri(EnsureTrailingSlash(baseUrl), apiPath), "../admin/health");

    private static Uri WasteObligationsHealth(string baseUrl, bool deep) =>
        new(EnsureTrailingSlash(baseUrl), deep ? "health/all" : "health");

    private static Uri EnsureTrailingSlash(string baseUrl) => new(baseUrl.TrimEnd('/') + "/", UriKind.Absolute);

    private static string SafeEndpoint(Uri endpoint)
    {
        var builder = new UriBuilder(endpoint) { UserName = string.Empty, Password = string.Empty, Query = string.Empty };
        return builder.Uri.ToString();
    }

    private static string? GetFailure(HttpResponseMessage response, bool includeResponse, JsonNode? body)
    {
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return "authentication";
        }

        if (includeResponse && body is null)
        {
            return "invalid_response";
        }

        return null;
    }

    private static DownstreamHealthResult CreateFailureResult(Exception exception, Uri? endpoint, long durationMs)
    {
        if (exception is UriFormatException || endpoint is null)
        {
            return new DownstreamHealthResult(Unhealthy, "not configured", null, durationMs, Failure: "configuration");
        }

        return new DownstreamHealthResult(Unhealthy, SafeEndpoint(endpoint), null, durationMs, Failure: FailureFor(exception));
    }

    private static string FailureFor(Exception exception) => exception switch
    {
        OperationCanceledException => "timeout",
        AuthenticationFailedException => "authentication",
        _ => "unavailable",
    };

    private async Task<(string Name, DownstreamHealthResult Result)> CheckAsync(
        string name,
        string clientName,
        Func<Uri> endpointFactory,
        bool includeResponse,
        int? hop,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, aggregateHealthOptions.Value.DownstreamTimeoutSeconds)));
        Uri? endpoint = null;

        try
        {
            endpoint = endpointFactory();
            using var client = httpClientFactory.CreateClient(clientName);
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            if (hop.HasValue)
            {
                AggregateHealthHop.AddTo(request, hop.Value);
            }

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            var body = includeResponse ? await ReadJsonResponseAsync(response, timeout.Token) : null;
            var failure = GetFailure(response, includeResponse, body);
            var isHealthy = response.IsSuccessStatusCode && failure is null;

            return (name, new DownstreamHealthResult(
                isHealthy ? Healthy : Unhealthy,
                SafeEndpoint(endpoint),
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                body,
                failure));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return (name, CreateFailureResult(exception, endpoint, stopwatch.ElapsedMilliseconds));
        }
    }

    private async Task<JsonNode?> ReadJsonResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var maxBytes = Math.Max(1, aggregateHealthOptions.Value.MaximumResponseBodyBytes);
        if (response.Content.Headers.ContentLength is long contentLength && contentLength > maxBytes)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var buffer = new MemoryStream();
        var readBuffer = new byte[Math.Min(81920, maxBytes + 1)];
        while (true)
        {
            var bytesToRead = (int)Math.Min(readBuffer.Length, maxBytes - buffer.Length + 1);
            var bytesRead = await stream.ReadAsync(readBuffer.AsMemory(0, bytesToRead), cancellationToken);
            if (bytesRead == 0)
            {
                break;
            }

            await buffer.WriteAsync(readBuffer.AsMemory(0, bytesRead), cancellationToken);
            if (buffer.Length > maxBytes)
            {
                return null;
            }
        }

        try
        {
            return JsonNode.Parse(Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length));
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

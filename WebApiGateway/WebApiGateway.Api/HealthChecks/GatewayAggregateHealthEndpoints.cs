using Microsoft.Extensions.Options;
using WebApiGateway.Core.Options;

namespace WebApiGateway.Api.HealthChecks;

public sealed class GatewayAggregateHealthEndpoints(
    IOptions<AccountApiOptions> accountApiOptions,
    IOptions<SubmissionStatusApiOptions> submissionStatusApiOptions,
    IOptions<PaymentServiceOptions> paymentServiceOptions,
    IOptions<PrnServiceApiOptions> prnServiceApiOptions,
    IOptions<CommonDataApiOptions> commonDataApiOptions,
    IOptions<WasteObligationsOptions> wasteObligationsOptions)
{
    public string AccountApiBaseUrl => accountApiOptions.Value.BaseUrl;

    public string SubmissionStatusApiBaseUrl => submissionStatusApiOptions.Value.BaseUrl;

    public string PaymentServiceBaseUrl => paymentServiceOptions.Value.BaseUrl;

    public string PrnServiceApiBaseUrl => prnServiceApiOptions.Value.BaseUrl;

    public string CommonDataApiBaseUrl => commonDataApiOptions.Value.BaseUrl;

    public string WasteObligationsBaseAddress => wasteObligationsOptions.Value.BaseAddress;
}

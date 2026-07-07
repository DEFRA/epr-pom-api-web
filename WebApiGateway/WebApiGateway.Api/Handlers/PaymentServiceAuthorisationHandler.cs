using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using WebApiGateway.Core.Options;

namespace WebApiGateway.Api.Handlers;

[ExcludeFromCodeCoverage]
public class PaymentServiceAuthorisationHandler(IOptions<PaymentServiceOptions> options)
    : AzureTokenAuthorisationHandler(options.Value.ClientId);

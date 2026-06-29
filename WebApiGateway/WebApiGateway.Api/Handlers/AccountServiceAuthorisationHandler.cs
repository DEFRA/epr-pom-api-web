using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using WebApiGateway.Core.Options;

namespace WebApiGateway.Api.Handlers;

[ExcludeFromCodeCoverage]
public class AccountServiceAuthorisationHandler(IOptions<AccountApiOptions> options)
    : AzureTokenAuthorisationHandler(options.Value.ClientId);

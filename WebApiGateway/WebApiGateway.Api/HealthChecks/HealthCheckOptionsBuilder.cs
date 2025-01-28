using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApiGateway.Api.HealthChecks;

[ExcludeFromCodeCoverage]
public static class HealthCheckOptionsBuilder
{
    public static HealthCheckOptions Build()
    {
        return new HealthCheckOptions
        {
            AllowCachingResponses = false,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK
            }
        };
    }
}
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Constants;

namespace WebApiGateway.Api.Services;

public class ComplianceSchemeDetailsService(IHttpContextAccessor httpContextAccessor)
    : IComplianceSchemeDetailsService
{
    public async Task<Guid?> GetComplianceSchemeIdAsync()
    {
        var context = httpContextAccessor.HttpContext;
        if (context != null && context.Items.TryGetValue(ComplianceScheme.ComplianceSchemeId, out var value))
        {
            // Simulate an asynchronous operation if needed
            await Task.CompletedTask;
            if (value is Guid complianceSchemeId)
            {
                return complianceSchemeId;
            }

            if (value is string complianceSchemeIdString && Guid.TryParse(complianceSchemeIdString, out var parsedComplianceSchemeId))
            {
                return parsedComplianceSchemeId;
            }
        }

        return null;
    }
}
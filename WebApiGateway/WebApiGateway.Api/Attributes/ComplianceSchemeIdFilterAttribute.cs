using Microsoft.AspNetCore.Mvc.Filters;
using WebApiGateway.Core.Constants;

namespace WebApiGateway.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ComplianceSchemeIdFilterAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check if the incoming request headers contain the "ComplianceSchemeId" header
            if (context.HttpContext.Request.Headers.TryGetValue(ComplianceScheme.ComplianceSchemeId, out var complianceSchemeId) &&
                !string.IsNullOrWhiteSpace(complianceSchemeId))
            {
                // Add the ComplianceSchemeId to HttpContext.Items for use in the controller actions
                context.HttpContext.Items[ComplianceScheme.ComplianceSchemeId] = complianceSchemeId.ToString();
            }

            // Proceed with the next action in the pipeline
            await next();
        }
    }
}

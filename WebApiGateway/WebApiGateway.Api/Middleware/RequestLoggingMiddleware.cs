using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Api.Middleware;

[ExcludeFromCodeCoverage]
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var request = context.Request;

        logger.LogInformation("Auth Request Path: {Path}", request.Path);

        foreach (var header in request.Headers)
        {
            logger.LogInformation("Header: {Key} {Value}", header.Key, header.Value.FirstOrDefault());
        }

        await next(context);

        var user = context.User;

        logger.LogInformation("IsAuthenticated: {IsAuth}", user.Identity?.IsAuthenticated);
        logger.LogInformation("AuthType: {AuthType}", user.Identity?.AuthenticationType);

        if (user.Identity?.IsAuthenticated == true)
        {
            foreach (var claim in user.Claims)
            {
                logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
        }

        var endpoint = context.GetEndpoint();
        var authData = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>();

        if (authData != null)
        {
            logger.LogInformation("Endpoint requires authorization policy: {Policy}", authData.Policy);
        }
    }
}
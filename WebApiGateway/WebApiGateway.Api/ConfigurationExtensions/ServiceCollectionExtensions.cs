using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Api.Clients;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Handlers;
using WebApiGateway.Api.Services;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Options;

namespace WebApiGateway.Api.ConfigurationExtensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<ISubmissionStatusClient, SubmissionStatusClient>();
        services.AddScoped<IAntivirusClient, AntivirusClient>();
        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddScoped<IAntivirusService, AntivirusService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<AccountServiceAuthorisationHandler>();
        services.AddScoped<AntivirusApiAuthorizationHandler>();
        services.AddScoped<IDecisionService, DecisionService>();

        return services;
    }

    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AccountApiOptions>(configuration.GetSection(AccountApiOptions.Section));
        services.Configure<SubmissionStatusApiOptions>(configuration.GetSection(SubmissionStatusApiOptions.Section));
        services.Configure<AntivirusApiOptions>(configuration.GetSection(AntivirusApiOptions.Section));
        services.Configure<StorageAccountOptions>(configuration.GetSection(StorageAccountOptions.Section));
        services.Configure<DecisionApiOptions>(configuration.GetSection(DecisionApiOptions.Section));

        return services;
    }
}
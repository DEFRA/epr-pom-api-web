namespace WebApiGateway.Api.ConfigurationExtensions;

using System.Diagnostics.CodeAnalysis;
using Clients;
using Clients.Interfaces;
using Core.Options;
using Handlers;
using Services;
using Services.Interfaces;

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
        services.AddScoped<IDecisionService, DecisionService>();
        services.AddScoped<AccountServiceAuthorisationHandler>();
        services.AddScoped<AntivirusApiAuthorizationHandler>();

        return services;
    }

    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AccountApiOptions>(configuration.GetSection(AccountApiOptions.Section));
        services.Configure<SubmissionStatusApiOptions>(configuration.GetSection(SubmissionStatusApiOptions.Section));
        services.Configure<DecisionApiOptions>(configuration.GetSection(DecisionApiOptions.Section));
        services.Configure<AntivirusApiOptions>(configuration.GetSection(AntivirusApiOptions.Section));
        services.Configure<StorageAccountOptions>(configuration.GetSection(StorageAccountOptions.Section));

        return services;
    }
}
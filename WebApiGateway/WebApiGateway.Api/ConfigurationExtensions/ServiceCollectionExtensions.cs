﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
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
        var sp = services.BuildServiceProvider();
        var redisConfig = sp.GetRequiredService<IOptions<RedisConfig>>().Value;
        services.AddScoped<ISubmissionStatusClient, SubmissionStatusClient>();
        services.AddScoped<IAntivirusClient, AntivirusClient>();
        services.AddScoped<IFileUploadService, FileUploadService>();
        services.AddScoped<IAntivirusService, AntivirusService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<AccountServiceAuthorisationHandler>();
        services.AddScoped<AntivirusApiAuthorizationHandler>();
        services.AddScoped<IDecisionService, DecisionService>();
        services.AddScoped<ISubsidiaryService, SubsidiaryService>();
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig.ConnectionString));
        services.AddScoped<ISubsidiariesService, SubsidiariesService>();
        services.AddScoped<PrnServiceAuthorisationHandler>();
        services.AddScoped<IPrnService, PrnService>();

        return services;
    }

    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AccountApiOptions>(configuration.GetSection(AccountApiOptions.Section));
        services.Configure<SubmissionStatusApiOptions>(configuration.GetSection(SubmissionStatusApiOptions.Section));
        services.Configure<AntivirusApiOptions>(configuration.GetSection(AntivirusApiOptions.Section));
        services.Configure<StorageAccountOptions>(configuration.GetSection(StorageAccountOptions.Section));
        services.Configure<DecisionApiOptions>(configuration.GetSection(DecisionApiOptions.Section));
        services.Configure<RedisConfig>(configuration.GetSection(RedisConfig.SectionName));
        services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.Section));
        services.Configure<PrnServiceApiOptions>(configuration.GetSection(PrnServiceApiOptions.Section));

        return services;
    }
}
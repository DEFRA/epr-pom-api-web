﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using WebApiGateway.Api.Clients;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Handlers;
using WebApiGateway.Core.Options;

namespace WebApiGateway.Api.ConfigurationExtensions;

[ExcludeFromCodeCoverage]
public static class HttpClientServiceCollectionExtensions
{
    public static IServiceCollection RegisterHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<ISubmissionStatusClient, SubmissionStatusClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<SubmissionStatusApiOptions>>().Value;
                client.BaseAddress = new Uri($"{options.BaseUrl}/v1/");
            })
            .AddPolicyHandler(GetRetryPolicy());

        var antivirusOptions = services.BuildServiceProvider().GetRequiredService<IOptions<AntivirusApiOptions>>().Value;

        if (antivirusOptions.EnableDirectAccess)
        {
            services.AddHttpClient<IAntivirusClient, AntivirusClient>(client =>
            {
                client.BaseAddress = new Uri($"{antivirusOptions.BaseUrl}/");
                client.Timeout = TimeSpan.FromSeconds(antivirusOptions.Timeout);
            });
        }
        else
        {
            services.AddHttpClient<IAntivirusClient, AntivirusClient>(client =>
            {
                client.BaseAddress = new Uri($"{antivirusOptions.BaseUrl}/v1/");
                client.Timeout = TimeSpan.FromSeconds(antivirusOptions.Timeout);
                client.DefaultRequestHeaders.Add("OCP-APIM-Subscription-Key", antivirusOptions.SubscriptionKey);
            }).AddHttpMessageHandler<AntivirusApiAuthorizationHandler>();
        }

        services.AddHttpClient<IAccountServiceClient, AccountServiceClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<AccountApiOptions>>().Value;
                client.BaseAddress = new Uri($"{options.BaseUrl}/api/");
                client.Timeout = TimeSpan.FromSeconds(options.Timeout);
            })
            .AddHttpMessageHandler<AccountServiceAuthorisationHandler>()
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<IDecisionClient, DecisionClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<DecisionApiOptions>>().Value;
                client.BaseAddress = new Uri($"{options.BaseUrl}/v1/");
            })
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<IPrnServiceClient, PrnServiceClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<PrnServiceApiOptions>>().Value;
                client.BaseAddress = new Uri($"{options.BaseUrl}/api/");
            })
            .AddHttpMessageHandler<PrnServiceAuthorisationHandler>()
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<IRegistrationFeeCalculationDetailsClient, RegistrationFeeCalculationDetailsClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<CommonDataApiOptions>>().Value;
                client.BaseAddress = new Uri($"{options.BaseUrl}/api/");
            })
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<ICommondataClient, CommondataClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<CommonDataApiOptions>>().Value;
                client.BaseAddress = new Uri($"{options.BaseUrl}/api/");
            })
            .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy() => HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt)));
}
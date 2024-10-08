﻿namespace WebApiGateway.Api.Clients;

using System;
using System.Net.Http;
using Core.Models.Events;
using Core.Models.ProducerValidation;
using Core.Models.Submission;
using Extensions;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.RegistrationValidation;
using WebApiGateway.Core.Models.SubmissionHistory;
using WebApiGateway.Core.Models.Submissions;
using WebApiGateway.Core.Models.UserAccount;

public class SubmissionStatusClient : ISubmissionStatusClient
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SubmissionStatusClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly IAccountServiceClient _accountServiceClient;

    public SubmissionStatusClient(
        HttpClient httpClient,
        IAccountServiceClient accountServiceClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SubmissionStatusClient> logger)
    {
        _httpClient = httpClient;
        _accountServiceClient = accountServiceClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task CreateEventAsync(AntivirusCheckEvent @event, Guid submissionId)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await _httpClient.PostAsJsonAsync($"submissions/{submissionId}/events", @event);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error creating {eventType} event", @event.Type);
            throw;
        }
    }

    public async Task CreateSubmissionAsync(CreateSubmission submission)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await _httpClient.PostAsJsonAsync("submissions", submission);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error creating submission");
            throw;
        }
    }

    public async Task<HttpResponseMessage> GetSubmissionAsync(Guid submissionId)
    {
        await ConfigureHttpClientAsync();

        return await _httpClient.GetAsync($"submissions/{submissionId}");
    }

    public async Task<List<AbstractSubmission>> GetSubmissionsAsync(string queryString)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await _httpClient.GetAsync($"submissions{queryString}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<AbstractSubmission>>(content);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error getting submissions");
            throw;
        }
    }

    public async Task<List<RegistrationValidationError>> GetRegistrationValidationErrorsAsync(Guid submissionId)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await _httpClient.GetAsync($"submissions/{submissionId}/organisation-details-errors");

            response.EnsureSuccessStatusCode();

            var errors = await response.Content.ReadFromJsonAsync<List<RegistrationValidationError>>();

            return errors;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error getting registration validation errors");
            throw;
        }
    }

    public async Task<List<ProducerValidationIssueRow>> GetProducerValidationErrorRowsAsync(Guid submissionId)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await _httpClient.GetAsync($"submissions/{submissionId}/producer-validations");

            response.EnsureSuccessStatusCode();

            var errors = await response.Content.ReadFromJsonAsync<List<ProducerValidationIssueRow>>();

            return errors.Select(error =>
            {
                error.Issue = IssueType.Error;
                return error;
            }).ToList();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error getting producer validation errors");
            throw;
        }
    }

    public async Task<List<ProducerValidationIssueRow>> GetProducerValidationWarningRowsAsync(Guid submissionId)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await _httpClient.GetAsync($"submissions/{submissionId}/producer-warning-validations");

            response.EnsureSuccessStatusCode();

            var warnings = await response.Content.ReadFromJsonAsync<List<ProducerValidationIssueRow>>();

            return warnings.Select(warning =>
            {
                warning.Issue = IssueType.Warning;
                return warning;
            }).ToList();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error getting producer validation warnings");
            throw;
        }
    }

    public async Task SubmitAsync(Guid submissionId, SubmissionPayload submissionPayload)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await _httpClient.PostAsJsonAsync($"submissions/{submissionId}/submit", submissionPayload);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error submitting submission with id {submissionId} and file id {fileId}", submissionId, submissionPayload.FileId);
            throw;
        }
    }

    public async Task<SubmissionHistoryEventsResponse> GetSubmissionPeriodHistory(Guid submissionId, string queryString)
    {
        await ConfigureHttpClientAsync();

        var response = await _httpClient.GetAsync($"submissions/events/events-by-type/{submissionId}{queryString}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var results = JsonConvert.DeserializeObject<SubmissionHistoryEventsResponse>(content);

        foreach (var submitted in results.SubmittedEvents)
        {
            var userDetails = await _accountServiceClient.GetUserAccount(submitted.UserId);

            if (userDetails == null)
            {
                _logger.LogError("Error searching for user with id {userId}", submitted.UserId);
            }
            else
            {
                submitted.SubmittedBy = userDetails.User.FirstName + " " + userDetails.User.LastName;
            }
        }

        return results;
    }

    public async Task<List<SubmissionGetResponse>> GetSubmissionsByFilter(Guid organisationId, Guid? complianceSchemeId, int? year, SubmissionType submissionType)
    {
        await ConfigureHttpClientAsync();

        string endpoint = $"submissions/submissions?Type={submissionType}&OrganisationId={organisationId}";

        if (complianceSchemeId is not null && complianceSchemeId != Guid.Empty)
        {
            endpoint += $"&ComplianceSchemeId={complianceSchemeId}";
        }

        if (year is not null)
        {
            endpoint += $"&Year={year}";
        }

        var response = await _httpClient.GetAsync(endpoint);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<SubmissionGetResponse>>(content);
    }

    private async Task ConfigureHttpClientAsync()
    {
        Guid userId = Guid.NewGuid();
        UserAccount userAccount;
        OrganisationDetail organisation;

        try
        {
            userId = _httpContextAccessor.HttpContext.User.UserId();
            userAccount = await _accountServiceClient.GetUserAccount(userId);
            organisation = userAccount.User.Organisations[0];
            _httpClient.DefaultRequestHeaders.AddIfNotExists("OrganisationId", organisation.Id.ToString());
            _httpClient.DefaultRequestHeaders.AddIfNotExists("UserId", userId.ToString());
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error getting user accounts with id {UserId}", userId);
            throw;
        }
    }
}
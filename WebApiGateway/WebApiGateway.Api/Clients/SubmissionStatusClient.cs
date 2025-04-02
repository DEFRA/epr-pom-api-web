using System.Net;
using EPR.SubmissionMicroservice.Application.Features.Queries.Common;
using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.ProducerValidation;
using WebApiGateway.Core.Models.RegistrationValidation;
using WebApiGateway.Core.Models.Submission;
using WebApiGateway.Core.Models.SubmissionHistory;
using WebApiGateway.Core.Models.Submissions;

namespace WebApiGateway.Api.Clients;

public class SubmissionStatusClient(
    HttpClient httpClient,
    IAccountServiceClient accountServiceClient,
    IHttpContextAccessor httpContextAccessor,
    ILogger<SubmissionStatusClient> logger)
    : ISubmissionStatusClient
{
    public async Task CreateEventAsync(AntivirusCheckEvent @event, Guid submissionId)
    {
        var responseContent = string.Empty;
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.PostAsJsonAsync($"submissions/{submissionId}/events", @event);

            responseContent = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error creating {eventType} event, responseContent {responseContent}", @event.Type, responseContent);
            throw;
        }
    }

    public async Task CreateEventAsync<T>(T @event, Guid submissionId)
        where T : AbstractEvent
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.PostAsJsonAsync($"submissions/{submissionId}/events", @event);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error creating {eventType} event", @event.Type);
            throw;
        }
    }

    public async Task CreateApplicationSubmittedEventAsync(RegistrationApplicationSubmittedEvent registrationEvent, Guid submissionId)
    {
        var responseContent = string.Empty;
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.PostAsJsonAsync($"submissions/{submissionId}/events", registrationEvent);

            responseContent = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error creating {eventType} event, responseContent {responseContent}", registrationEvent.Type, responseContent);
            throw;
        }
    }

    public async Task CreateRegistrationFeePaymentEventAsync(RegistrationFeePaymentEvent registrationEvent, Guid submissionId)
    {
        var responseContent = string.Empty;
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.PostAsJsonAsync($"submissions/{submissionId}/events", registrationEvent);

            responseContent = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error creating {eventType} event, responseContent {responseContent}", registrationEvent.Type, responseContent);
            throw;
        }
    }

    public async Task CreateSubmissionAsync(CreateSubmission submission)
    {
        var responseContent = string.Empty;
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.PostAsJsonAsync("submissions", submission);

            responseContent = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error creating submission, responseContent {responseContent}", responseContent);
            throw;
        }
    }

    public async Task<HttpResponseMessage> GetSubmissionAsync(Guid submissionId)
    {
        await ConfigureHttpClientAsync();

        return await httpClient.GetAsync($"submissions/{submissionId}");
    }

    public async Task<List<AbstractSubmission>> GetSubmissionsAsync(string queryString)
    {
        var responseContent = string.Empty;
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.GetAsync($"submissions{queryString}");

            responseContent = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<AbstractSubmission>>(content);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error getting submissions, responseContent {responseContent}", responseContent);
            throw;
        }
    }

    public async Task<List<RegistrationValidationError>> GetRegistrationValidationErrorsAsync(Guid submissionId)
    {
        var responseContent = string.Empty;
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.GetAsync($"submissions/{submissionId}/organisation-details-errors");

            responseContent = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var errors = await response.Content.ReadFromJsonAsync<List<RegistrationValidationError>>();

            return errors.Select(error =>
            {
                error.IssueType = IssueType.Error;
                return error;
            }).ToList();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error getting registration validation errors, responseContent {responseContent}", responseContent);
            throw;
        }
    }

    public async Task<List<RegistrationValidationError>> GetRegistrationValidationWarningsAsync(Guid submissionId)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.GetAsync($"submissions/{submissionId}/organisation-details-warnings");

            response.EnsureSuccessStatusCode();

            var warnings = await response.Content.ReadFromJsonAsync<List<RegistrationValidationError>>();

            return warnings.Select(warning =>
            {
                warning.IssueType = IssueType.Warning;
                return warning;
            }).ToList();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error getting registration validation warnings");
            throw;
        }
    }

    public async Task<List<ProducerValidationIssueRow>> GetProducerValidationErrorRowsAsync(Guid submissionId)
    {
        var responseContent = string.Empty;
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.GetAsync($"submissions/{submissionId}/producer-validations");

            responseContent = await response.Content.ReadAsStringAsync();

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
            logger.LogError(exception, "Error getting producer validation errors, responseContent {responseContent}", responseContent);
            throw;
        }
    }

    public async Task<List<ProducerValidationIssueRow>> GetProducerValidationWarningRowsAsync(Guid submissionId)
    {
        var responseContent = string.Empty;
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.GetAsync($"submissions/{submissionId}/producer-warning-validations");

            responseContent = await response.Content.ReadAsStringAsync();

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
            logger.LogError(exception, "Error getting producer validation warnings, responseContent {responseContent}", responseContent);
            throw;
        }
    }

    public async Task SubmitAsync(Guid submissionId, SubmissionPayload submissionPayload)
    {
        var responseContent = string.Empty;
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.PostAsJsonAsync($"submissions/{submissionId}/submit", submissionPayload);

            responseContent = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error submitting submission with id {submissionId} and file id {fileId}, responseContent {responseContent}", submissionId, submissionPayload.FileId, responseContent);
            throw;
        }
    }

    public async Task<SubmissionHistoryEventsResponse> GetSubmissionPeriodHistory(Guid submissionId, string queryString)
    {
        await ConfigureHttpClientAsync();

        var response = await httpClient.GetAsync($"submissions/events/events-by-type/{submissionId}{queryString}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var results = JsonConvert.DeserializeObject<SubmissionHistoryEventsResponse>(content);

        foreach (var submitted in results.SubmittedEvents)
        {
            var userDetails = await accountServiceClient.GetUserAccount(submitted.UserId);

            if (userDetails == null!)
            {
                logger.LogError("Error searching for user with id {userId}", submitted.UserId);
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

        var endpoint = $"submissions/submissions?Type={submissionType}&OrganisationId={organisationId}";

        if (complianceSchemeId is not null && complianceSchemeId != Guid.Empty)
        {
            endpoint += $"&ComplianceSchemeId={complianceSchemeId}";
        }

        if (year is not null)
        {
            endpoint += $"&Year={year}";
        }

        var response = await httpClient.GetAsync(endpoint);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<SubmissionGetResponse>>(content);
    }

    public async Task<RegistrationApplicationDetails?> GetRegistrationApplicationDetails(string queryString)
    {
        await ConfigureHttpClientAsync();

        var endpointUrl = $"submissions/get-registration-application-details{queryString}";

        var uri = ValidateUrl(endpointUrl);

        var response = await httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<RegistrationApplicationDetails>(content);
    }

    public async Task<HttpResponseMessage> CreateFileDownloadEventAsync(FileDownloadCheckEvent fileDownloadCheckEvent, Guid submissionId)
    {
        var responseContent = string.Empty;
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.PostAsJsonAsync($"submissions/{submissionId}/events", fileDownloadCheckEvent);

            responseContent = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            return response;
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error creating {EventType} event, responseContent {responseContent}", fileDownloadCheckEvent.Type, responseContent);
            throw;
        }
    }

    public async Task<AntivirusResultEvent> GetFileScanResultAsync(Guid submissionId, Guid fileId)
    {
        await ConfigureHttpClientAsync();

        var response = await httpClient.GetAsync($"submissions/{submissionId}/uploadedfile/{fileId}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<AntivirusResultEvent>(content);
    }

    public async Task<List<PackagingResubmissionApplicationDetails?>> GetPackagingResubmissionApplicationDetails(string queryString)
    {
        await ConfigureHttpClientAsync();

        var endpointUrl = $"submissions/get-packaging-data-resubmission-application-details{queryString}";

        var uri = ValidateUrl(endpointUrl);

        var response = await httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<PackagingResubmissionApplicationDetails>>(content);
    }

    private static Uri ValidateUrl(string endpointUrl)
    {
        var uri = new Uri(endpointUrl, UriKind.RelativeOrAbsolute);
        string[] allowedSchemes = { "https", "http" };
        string[] allowedDomains = { "localhost" };

        if (uri.IsAbsoluteUri && !allowedDomains.Contains(uri.Host) && !allowedSchemes.Contains(uri.Scheme))
        {
            throw new InvalidOperationException();
        }

        return uri;
    }

    private async Task ConfigureHttpClientAsync()
    {
        var userId = Guid.NewGuid();

        try
        {
            userId = httpContextAccessor.HttpContext.User.UserId();
            var userAccount = await accountServiceClient.GetUserAccount(userId);
            var organisation = userAccount.User.Organisations[0];
            httpClient.DefaultRequestHeaders.AddIfNotExists("OrganisationId", organisation.Id.ToString());
            httpClient.DefaultRequestHeaders.AddIfNotExists("UserId", userId.ToString());
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error getting user accounts with id {UserId}", userId);
            throw;
        }
    }
}
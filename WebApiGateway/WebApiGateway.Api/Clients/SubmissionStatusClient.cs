using System.Net;
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
using static System.Net.WebRequestMethods;

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
        try
        {
            await ConfigureHttpClientAsync();
            var response = await httpClient.PostAsJsonAsync($"submissions/{submissionId}/events", registrationEvent);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error creating {eventType} event", registrationEvent.Type);
            throw;
        }
    }

    public async Task CreateRegistrationFeePaymentEventAsync(RegistrationFeePaymentEvent registrationEvent, Guid submissionId)
    {
        try
        {
            await ConfigureHttpClientAsync();
            var response = await httpClient.PostAsJsonAsync($"submissions/{submissionId}/events", registrationEvent);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error creating {eventType} event", registrationEvent.Type);
            throw;
        }
    }

    public async Task CreateSubmissionAsync(CreateSubmission submission)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.PostAsJsonAsync("submissions", submission);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error creating submission");
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
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.GetAsync($"submissions{queryString}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<AbstractSubmission>>(content);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error getting submissions");
            throw;
        }
    }

    public async Task<List<RegistrationValidationError>> GetRegistrationValidationErrorsAsync(Guid submissionId)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.GetAsync($"submissions/{submissionId}/organisation-details-errors");

            response.EnsureSuccessStatusCode();

            var errors = await response.Content.ReadFromJsonAsync<List<RegistrationValidationError>>();

            return errors;
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error getting registration validation errors");
            throw;
        }
    }

    public async Task<List<ProducerValidationIssueRow>> GetProducerValidationErrorRowsAsync(Guid submissionId)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.GetAsync($"submissions/{submissionId}/producer-validations");

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
            logger.LogError(exception, "Error getting producer validation errors");
            throw;
        }
    }

    public async Task<List<ProducerValidationIssueRow>> GetProducerValidationWarningRowsAsync(Guid submissionId)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.GetAsync($"submissions/{submissionId}/producer-warning-validations");

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
            logger.LogError(exception, "Error getting producer validation warnings");
            throw;
        }
    }

    public async Task SubmitAsync(Guid submissionId, SubmissionPayload submissionPayload)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.PostAsJsonAsync($"submissions/{submissionId}/submit", submissionPayload);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error submitting submission with id {submissionId} and file id {fileId}", submissionId, submissionPayload.FileId);
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

    public async Task<GetRegistrationApplicationDetailsResponse?> GetRegistrationApplicationDetails(string queryString)
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

        return JsonConvert.DeserializeObject<GetRegistrationApplicationDetailsResponse>(content);
    }

    public async Task<HttpResponseMessage> CreateFileDownloadEventAsync(FileDownloadCheckEvent fileDownloadCheckEvent, Guid submissionId)
    {
        try
        {
            await ConfigureHttpClientAsync();

            var response = await httpClient.PostAsJsonAsync($"submissions/{submissionId}/events", fileDownloadCheckEvent);
            response.EnsureSuccessStatusCode();

            return response;
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error creating {EventType} event", fileDownloadCheckEvent.Type);
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
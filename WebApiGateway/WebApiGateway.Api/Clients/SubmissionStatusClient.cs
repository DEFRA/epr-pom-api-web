namespace WebApiGateway.Api.Clients;

using Core.Models.Events;
using Core.Models.ProducerValidation;
using Core.Models.Submission;
using Extensions;
using Interfaces;
using Newtonsoft.Json;
using WebApiGateway.Core.Constants;

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

    private async Task ConfigureHttpClientAsync()
    {
        var userId = _httpContextAccessor.HttpContext.User.UserId();
        var userAccount = await _accountServiceClient.GetUserAccount(userId);
        var organisation = userAccount.User.Organisations.First();

        _httpClient.DefaultRequestHeaders.AddIfNotExists("OrganisationId", organisation.Id.ToString());
        _httpClient.DefaultRequestHeaders.AddIfNotExists("UserId", userId.ToString());
    }
}
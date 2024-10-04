using System.Text.Json;
using StackExchange.Redis;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Models.Subsidiary;

namespace WebApiGateway.Api.Services;

public class SubsidiaryService(
    IAccountServiceClient accountServiceClient,
    IHttpContextAccessor httpContextAccessor,
    IConnectionMultiplexer redisConnectionMultiplexer,
    ILogger<SubsidiaryService> logger) : ISubsidiaryService
{
    private readonly IAccountServiceClient _accountServiceClient = accountServiceClient;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<SubsidiaryService> _logger = logger;
    private readonly IDatabase _redisDatabase = redisConnectionMultiplexer.GetDatabase();

    private readonly JsonSerializerOptions _caseInsensitiveJsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<UploadFileErrorResponse> GetNotificationErrorsAsync(string key)
    {
        var value = await _redisDatabase.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            _logger.LogInformation("Redis empty errors response key: {Key}", key);

            return new UploadFileErrorResponse();
        }

        _logger.LogInformation("Redis errors response key: {Key} errors: {Value}", key, value);

        return JsonSerializer.Deserialize<UploadFileErrorResponse>(value, _caseInsensitiveJsonSerializerOptions);
    }

    public async Task<string> GetNotificationStatusAsync(string key)
    {
        var value = await _redisDatabase.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            _logger.LogInformation("Redis empty status response key: {Key}", key);

            return string.Empty;
        }

        _logger.LogInformation("Redis status response key: {Key} value: {Value}", key, value);

        return value;
    }

    public async Task InitializeUploadStatusAsync()
    {
        var status = "Uploading";

        var user = await GetUserAndOrganisationAsync();
        var key = $"{user.UserId}{user.OrganisationId}{SubsidiaryBulkUploadStatusKeys.SubsidiaryBulkUploadProgress}";
        var errorsKey = $"{user.UserId}{user.OrganisationId}{SubsidiaryBulkUploadStatusKeys.SubsidiaryBulkUploadErrors}";

        await _redisDatabase.StringSetAsync(key, status);
        await _redisDatabase.KeyDeleteAsync(errorsKey);

        _logger.LogInformation("Redis status: {Key} set to {Value} and any errors removed", key, status);
    }

    private async Task<(Guid UserId, Guid OrganisationId)> GetUserAndOrganisationAsync()
    {
        var userId = _httpContextAccessor.HttpContext.User.UserId();

        try
        {
            var userAccount = await _accountServiceClient.GetUserAccount(userId);
            var organisation = userAccount.User.Organisations[0];

            return (userId, organisation.Id);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error getting user accounts with id {UserId}", userId);
            throw;
        }
    }
}

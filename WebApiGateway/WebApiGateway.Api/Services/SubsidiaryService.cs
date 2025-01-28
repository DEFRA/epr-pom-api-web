using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Models.Subsidiary;
using WebApiGateway.Core.Options;

namespace WebApiGateway.Api.Services;

public class SubsidiaryService(
    IAccountServiceClient accountServiceClient,
    IHttpContextAccessor httpContextAccessor,
    IConnectionMultiplexer redisConnectionMultiplexer,
    IOptions<RedisOptions> redisOptions,
    ILogger<SubsidiaryService> logger) : ISubsidiaryService
{
    private readonly IDatabase _redisDatabase = redisConnectionMultiplexer.GetDatabase();
    private readonly RedisOptions _redisOptions = redisOptions.Value;

    private readonly JsonSerializerOptions _caseInsensitiveJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<UploadFileErrorResponse> GetNotificationErrorsAsync(string key)
    {
        var value = await _redisDatabase.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            logger.LogInformation("Redis empty errors response key: {Key}", key);

            return new UploadFileErrorResponse();
        }

        logger.LogInformation("Redis errors response key: {Key} errors: {Value}", key, value);

        return JsonSerializer.Deserialize<UploadFileErrorResponse>(value, _caseInsensitiveJsonSerializerOptions);
    }

    public async Task<string> GetNotificationStatusAsync(string key)
    {
        var value = await _redisDatabase.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            logger.LogInformation("Redis empty status response key: {Key}", key);

            return string.Empty;
        }

        logger.LogInformation("Redis status response key: {Key} value: {Value}", key, value);

        return value;
    }

    public async Task InitializeUploadStatusAsync()
    {
        var status = "Uploading";

        var user = await GetUserAndOrganisationAsync();
        var progressKey = $"{user.UserId}{user.OrganisationId}{SubsidiaryBulkUploadStatusKeys.SubsidiaryBulkUploadProgress}";
        var rowsAddedKey = $"{user.UserId}{user.OrganisationId}{SubsidiaryBulkUploadStatusKeys.SubsidiaryBulkUploadRowsAdded}";
        var errorsKey = $"{user.UserId}{user.OrganisationId}{SubsidiaryBulkUploadStatusKeys.SubsidiaryBulkUploadErrors}";

        var expiry = _redisOptions.TimeToLiveInMinutes is not null
            ? TimeSpan.FromMinutes(_redisOptions.TimeToLiveInMinutes.Value)
            : default(TimeSpan?);

        await _redisDatabase.StringSetAsync(progressKey, status, expiry);
        await _redisDatabase.KeyDeleteAsync(rowsAddedKey);
        await _redisDatabase.KeyDeleteAsync(errorsKey);

        logger.LogInformation("Redis status: {Key} set to {Value}. Rows added and errors removed", progressKey, status);
    }

    private async Task<(Guid UserId, Guid OrganisationId)> GetUserAndOrganisationAsync()
    {
        var userId = httpContextAccessor.HttpContext.User.UserId();

        try
        {
            var userAccount = await accountServiceClient.GetUserAccount(userId);
            var organisation = userAccount.User.Organisations[0];

            return (userId, organisation.Id);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error getting user accounts with id {UserId}", userId);
            throw;
        }
    }
}

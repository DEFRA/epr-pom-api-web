using System.Text.Json;
using StackExchange.Redis;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Subsidiary;

namespace WebApiGateway.Api.Services;

public class SubsidiaryService(
    ILogger<SubsidiaryService> logger, IConnectionMultiplexer redisConnectionMultiplexer) : ISubsidiaryService
{
    private readonly ILogger<SubsidiaryService> _logger = logger;
    private readonly IDatabase _redisDatabase = redisConnectionMultiplexer.GetDatabase();

    public async Task<UploadFileErrorResponse> GetNotificationErrorsAsync(string key)
    {
        var value = await _redisDatabase.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            _logger.LogInformation("Redis empty errors response key: {Key}", key);

            return new UploadFileErrorResponse();
        }

        _logger.LogInformation("Redis errors response key: {Key} errors: {Value}", key, value);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<UploadFileErrorResponse>(value, options);
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
}

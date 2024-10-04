using WebApiGateway.Core.Models.Subsidiary;

namespace WebApiGateway.Api.Services.Interfaces;

public interface ISubsidiaryService
{
    Task<UploadFileErrorResponse> GetNotificationErrorsAsync(string key);

    Task<string> GetNotificationStatusAsync(string key);

    Task InitializeUploadStatusAsync();
}

using WebApiGateway.Core.Models.Subsidiaries;

namespace WebApiGateway.Api.Services.Interfaces;

public interface ISubsidiariesService
{
    Task<GetFileUploadTemplateResponse> GetFileUploadTemplateAsync();
}
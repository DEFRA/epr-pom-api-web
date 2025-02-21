using WebApiGateway.Core.Models.Submission;

namespace WebApiGateway.Api.Services.Interfaces;

public interface IRegistrationApplicationService
{
    Task<RegistrationApplicationDetails?> GetRegistrationApplicationDetails(string request);
}
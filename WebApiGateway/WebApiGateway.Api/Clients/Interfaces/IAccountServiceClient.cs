namespace WebApiGateway.Api.Clients.Interfaces;

using Core.Models.UserAccount;

public interface IAccountServiceClient
{
    Task<UserAccount> GetUserAccount(Guid userId);
}
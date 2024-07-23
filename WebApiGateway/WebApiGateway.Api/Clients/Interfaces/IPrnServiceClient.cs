using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Clients.Interfaces
{
    public interface IPrnServiceClient
    {
        Task<List<Prn>> GetAllPrnsForOrganisation(Guid organisationId);

        Task<Prn> GetPrnById(int id);

        Task UpdatePrnStatusToAccepted(int id);
    }
}

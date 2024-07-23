using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Clients.Interfaces
{
    public interface IPrnServiceClient
    {
        Task<List<PrnModel>> GetAllPrnsForOrganisation(Guid organisationId);

        Task<PrnModel> GetPrnById(int id);

        Task UpdatePrnStatusToAccepted(int id);
    }
}

using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Clients.Interfaces
{
    public interface IPrnServiceClient
    {
        Task<List<PrnModel>> GetAllPrnsForOrganisation();

        Task<PrnModel> GetPrnById(Guid id);

        Task UpdatePrnStatus(List<UpdatePrnStatus> updatePrns);
    }
}

using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Services.Interfaces
{
    public interface IPrnService
    {
        Task<List<PrnModel>> GetAllPrnsForOrganisation();

        Task<PrnModel> GetPrnById(Guid id);

        Task UpdatePrnStatus(List<UpdatePrnStatus> updatePrns);
    }
}

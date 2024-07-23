using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Services.Interfaces
{
    public interface IPrnService
    {
        Task<List<PrnModel>> GetAllPrnsForOrganisation(Guid organisationId);

        Task<PrnModel> GetPrnById(int id);

        Task UpdatePrnStatusToAccepted(int id);
    }
}

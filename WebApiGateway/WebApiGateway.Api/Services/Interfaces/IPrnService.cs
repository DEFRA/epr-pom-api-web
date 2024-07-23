using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Services.Interfaces
{
    public interface IPrnService
    {
        Task<List<PrnModel>> GetAllPrnsForOrganisation(Guid organisationId);

        Task<PrnModel> GetPrnById(Guid id);

        Task UpdatePrnStatusToAccepted(Guid id);
    }
}

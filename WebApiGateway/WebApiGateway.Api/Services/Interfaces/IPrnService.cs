using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Services.Interfaces
{
    public interface IPrnService
    {
        Task<List<Prn>> GetAllPrnsForOrganisation(Guid organisationId);

        Task<Prn> GetPrnById(int id);

        Task UpdatePrnStatusToAccepted(int id);
    }
}

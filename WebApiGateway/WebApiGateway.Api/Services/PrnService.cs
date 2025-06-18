using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Pagination;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Services;

public class PrnService(IPrnServiceClient prnServiceClient, ILogger<PrnService> logger, IConfiguration config) : IPrnService
{
    private readonly string logPrefix = config["LogPrefix"];

    public async Task<List<PrnModel>> GetAllPrnsForOrganisation()
    {
        logger.LogInformation("{Logprefix}: PrnService - GetAllPrnsForOrganisation", logPrefix);
        return await prnServiceClient.GetAllPrnsForOrganisation();
    }

    public async Task<PrnModel> GetPrnById(Guid id)
    {
        logger.LogInformation("{Logprefix}: PrnService - GetPrnById for prn {PrnId}", logPrefix, id);
        return await prnServiceClient.GetPrnById(id);
    }

    public async Task UpdatePrnStatus(List<UpdatePrnStatus> updatePrns)
    {
        logger.LogInformation("{Logprefix}: PrnService - UpdatePrnStatus for given Prns {PrnIds}", logPrefix, JsonConvert.SerializeObject(updatePrns));
        await prnServiceClient.UpdatePrnStatus(updatePrns);
    }

    public async Task<PaginatedResponse<PrnModel>> GetSearchPrns(PaginatedRequest request)
    {
        logger.LogInformation("{Logprefix}: PrnService - GetSearchPrns for given search criteria {PrnIds}", logPrefix, JsonConvert.SerializeObject(request));
        return await prnServiceClient.GetSearchPrns(request);
    }

    public async Task<ObligationModel> GetObligationCalculationByYearAsync(int year)
    {
        logger.LogInformation("{Logprefix}: PrnService - GetObligationCalculationByYearAsync for {Year}", logPrefix, year);
        return await prnServiceClient.GetObligationCalculationByYearAsync(year);
    }
}
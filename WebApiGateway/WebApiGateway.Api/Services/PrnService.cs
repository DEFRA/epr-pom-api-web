using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Pagination;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Services;

public class PrnService : IPrnService
{
    private readonly IPrnServiceClient _prnServiceClient;
    private readonly ILogger<PrnService> _logger;
    private readonly string logPrefix;

    public PrnService(IPrnServiceClient prnServiceClient, ILogger<PrnService> logger, IConfiguration config)
    {
        _prnServiceClient = prnServiceClient;
        _logger = logger;
        logPrefix = config["LogPrefix"];
    }

    public async Task<List<PrnModel>> GetAllPrnsForOrganisation()
    {
        _logger.LogInformation("{Logprefix}: PrnService - GetAllPrnsForOrganisation", logPrefix);
        return await _prnServiceClient.GetAllPrnsForOrganisation();
    }

    public async Task<PrnModel> GetPrnById(Guid id)
    {
        _logger.LogInformation("{Logprefix}: PrnService - GetPrnById for prn {PrnId}", logPrefix, id);
        return await _prnServiceClient.GetPrnById(id);
    }

    public async Task UpdatePrnStatus(List<UpdatePrnStatus> updatePrns)
    {
        _logger.LogInformation("{Logprefix}: PrnService - UpdatePrnStatus for given Prns {PrnIds}", logPrefix, JsonConvert.SerializeObject(updatePrns));
        await _prnServiceClient.UpdatePrnStatus(updatePrns);
    }

    public async Task<PaginatedResponse<PrnModel>> GetSearchPrns(PaginatedRequest request)
    {
        _logger.LogInformation("{Logprefix}: PrnService - GetSearchPrns for given search criteria {PrnIds}", logPrefix, JsonConvert.SerializeObject(request));
        return await _prnServiceClient.GetSearchPrns(request);
    }

    public async Task<ObligationModel> GetObligationCalculationByYear(int year)
    {
        _logger.LogInformation("{Logprefix}: PrnService - GetObligationCalculationByYear for {Year}", logPrefix, year);
        return await _prnServiceClient.GetObligationCalculationByYearAsync(year);
    }
}
using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Pagination;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Clients;

public class PrnServiceClient : ServiceClientBase, IPrnServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PrnServiceClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccountServiceClient _accountServiceClient;
    private readonly IComplianceSchemeDetailsService _complianceSchemeSvc;
    private readonly string logPrefix;
    private readonly string obligationCalculationURL = "v1/prn/obligationcalculation";

    public PrnServiceClient(HttpClient httpClient, ILogger<PrnServiceClient> logger, IHttpContextAccessor httpContextAccessor, IAccountServiceClient accountServiceClient, IConfiguration config, IComplianceSchemeDetailsService complianceSchemeSvc)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _accountServiceClient = accountServiceClient;
        _complianceSchemeSvc = complianceSchemeSvc;
        logPrefix = config["LogPrefix"];
    }

    public async Task<List<PrnModel>> GetAllPrnsForOrganisation()
    {
        var orgId = await ConfigureHttpClientAsync();
        try
        {
            _logger.LogInformation("{Logprefix}: PrnServiceClient - GetAllPrnsForOrganisation: calling endpoint 'v1/prn/organisation' with organisation id {OrganisationId}", logPrefix, orgId);

            var response = await _httpClient.GetAsync("v1/prn/organisation");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("{Logprefix}: PrnServiceClient - GetAllPrnsForOrganisation: response from endpoint {Response}", logPrefix, content);
            return JsonConvert.DeserializeObject<List<PrnModel>>(content);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "{Logprefix}: PrnServiceClient - GetAllPrnsForOrganisation: An error occurred retrieving prns for organisation {OrganisationId}", logPrefix, orgId);
            throw;
        }
    }

    public async Task<PaginatedResponse<PrnModel>> GetSearchPrns(PaginatedRequest request)
    {
        var orgId = await ConfigureHttpClientAsync();
        try
        {
            _logger.LogInformation("{Logprefix}: PrnServiceClient - GetSearchPrns: calling endpoint 'v1/prn/search' with organisation id {OrganisationId} and Search criteria {Search}", logPrefix, orgId, JsonConvert.SerializeObject(request));

            var response = await _httpClient.GetAsync($"v1/prn/search{BuildUrlWithQueryString(request)}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("{Logprefix}: PrnServiceClient - GetSearchPrns: response from endpoint {Response}", logPrefix, content);
            return JsonConvert.DeserializeObject<PaginatedResponse<PrnModel>>(content);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "{Logprefix}: PrnServiceClient - GetSearchPrns: An error occurred retrieving PRN search result for organisation {OrganisationId}", logPrefix, orgId);
            throw;
        }
    }

    public async Task<PrnModel> GetPrnById(Guid id)
    {
        try
        {
            await ConfigureHttpClientAsync();
            _logger.LogInformation("{Logprefix}: PrnServiceClient - GetPrnById: calling endpoint 'v1/prn/{Id}'", logPrefix, id);
            var response = await _httpClient.GetAsync($"v1/prn/{id}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("{Logprefix}: PrnServiceClient - GetPrnById: response from endpoint {Response}", logPrefix, content);
            return JsonConvert.DeserializeObject<PrnModel>(content);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "{Logprefix}: PrnServiceClient - GetPrnById: An error occurred retrieving prns for Id {PrnId}", logPrefix, id);
            throw;
        }
    }

    public async Task UpdatePrnStatus(List<UpdatePrnStatus> updatePrns)
    {
        try
        {
            await ConfigureHttpClientAsync();
            _logger.LogInformation("{Logprefix}: PrnServiceClient - UpdatePrnStatus: calling endpoint 'v1/prn/status' with Prns to update {UpdatePrns}", logPrefix, JsonConvert.SerializeObject(updatePrns));
            var response = await _httpClient.PostAsJsonAsync($"v1/prn/status", updatePrns);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "{Logprefix}: PrnServiceClient - UpdatePrnStatus: An error occurred updating prns statuses {UpdatePrns}", logPrefix, JsonConvert.SerializeObject(updatePrns));
            throw;
        }
    }

    public async Task<ObligationModel> GetObligationCalculationByYearAsync(int year)
    {
        try
        {
            var orgId = await ConfigureHttpClientAsync();
            _logger.LogInformation("{Logprefix}: PrnServiceClient - GetObligationCalculationByYearAsync: calling endpoint '{ObligationCalculationURL}/{Year}' for organisation {OrgId}", logPrefix, obligationCalculationURL, year, orgId);
            var response = await _httpClient.GetAsync($"{obligationCalculationURL}/{year}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("{Logprefix}: PrnServiceClient - GetObligationCalculationByYearAsync: response from endpoint {Response}", logPrefix, content);
            return JsonConvert.DeserializeObject<ObligationModel>(content);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "{Logprefix}: PrnServiceClient - GetObligationCalculationByYearAsync: An error occurred retrievig obligation calculations for organisation {ObligationCalculationURL}/{Year}", logPrefix, obligationCalculationURL, year);
            throw;
        }
    }

    private async Task<string> ConfigureHttpClientAsync()
    {
        var userId = _httpContextAccessor.HttpContext.User.UserId();

        try
        {
            var complianceSchemeId = await _complianceSchemeSvc.GetComplianceSchemeIdAsync();
            var organisationId = complianceSchemeId ?? (await _accountServiceClient.GetUserAccount(userId)).User.Organisations[0].Id;

            _httpClient.DefaultRequestHeaders.AddIfNotExists("X-EPR-ORGANISATION", organisationId.ToString());
            _httpClient.DefaultRequestHeaders.AddIfNotExists("X-EPR-USER", userId.ToString());

            return organisationId.ToString();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "{Logprefix}: PrnServiceClient - ConfigureHttpClientAsync: Error getting user accounts with id {UserId}", logPrefix, userId);
            throw;
        }
    }
}

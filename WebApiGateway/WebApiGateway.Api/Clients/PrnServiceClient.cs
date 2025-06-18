using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Models.Pagination;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Clients;

public class PrnServiceClient(HttpClient httpClient, ILogger<PrnServiceClient> logger, IHttpContextAccessor httpContextAccessor, IAccountServiceClient accountServiceClient, IConfiguration config)
    : ServiceClientBase, IPrnServiceClient
{
    private const string ObligationCalculationUrl = "v1/prn/obligationcalculation";

    private readonly string _logPrefix = config["LogPrefix"];

    public async Task<List<PrnModel>> GetAllPrnsForOrganisation()
    {
        var orgId = await ConfigureHttpClientAsync();
        try
        {
            logger.LogInformation("{_logPrefix}: PrnServiceClient - GetAllPrnsForOrganisation: calling endpoint 'v1/prn/organisation' with organisation id {OrganisationId}", _logPrefix, orgId);

            var response = await httpClient.GetAsync("v1/prn/organisation");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            logger.LogInformation("{_logPrefix}: PrnServiceClient - GetAllPrnsForOrganisation: response from endpoint {Response}", _logPrefix, content);
            return JsonConvert.DeserializeObject<List<PrnModel>>(content);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "{_logPrefix}: PrnServiceClient - GetAllPrnsForOrganisation: An error occurred retrieving prns for organisation {OrganisationId}", _logPrefix, orgId);
            throw;
        }
    }

    public async Task<PaginatedResponse<PrnModel>> GetSearchPrns(PaginatedRequest request)
    {
        var orgId = await ConfigureHttpClientAsync();
        try
        {
            logger.LogInformation("{_logPrefix}: PrnServiceClient - GetSearchPrns: calling endpoint 'v1/prn/search' with organisation id {OrganisationId} and Search criteria {Search}", _logPrefix, orgId, JsonConvert.SerializeObject(request));

            var response = await httpClient.GetAsync($"v1/prn/search{BuildUrlWithQueryString(request)}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            logger.LogInformation("{_logPrefix}: PrnServiceClient - GetSearchPrns: response from endpoint {Response}", _logPrefix, content);
            return JsonConvert.DeserializeObject<PaginatedResponse<PrnModel>>(content);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "{_logPrefix}: PrnServiceClient - GetSearchPrns: An error occurred retrieving PRN search result for organisation {OrganisationId}", _logPrefix, orgId);
            throw;
        }
    }

    public async Task<PrnModel> GetPrnById(Guid id)
    {
        try
        {
            await ConfigureHttpClientAsync();
            logger.LogInformation("{_logPrefix}: PrnServiceClient - GetPrnById: calling endpoint 'v1/prn/{Id}'", _logPrefix, id);
            var response = await httpClient.GetAsync($"v1/prn/{id}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            logger.LogInformation("{_logPrefix}: PrnServiceClient - GetPrnById: response from endpoint {Response}", _logPrefix, content);
            return JsonConvert.DeserializeObject<PrnModel>(content);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "{_logPrefix}: PrnServiceClient - GetPrnById: An error occurred retrieving prns for Id {PrnId}", _logPrefix, id);
            throw;
        }
    }

    public async Task UpdatePrnStatus(List<UpdatePrnStatus> updatePrns)
    {
        try
        {
            await ConfigureHttpClientAsync();
            logger.LogInformation("{_logPrefix}: PrnServiceClient - UpdatePrnStatus: calling endpoint 'v1/prn/status' with Prns to update {UpdatePrns}", _logPrefix, JsonConvert.SerializeObject(updatePrns));
            var response = await httpClient.PostAsJsonAsync($"v1/prn/status", updatePrns);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "{_logPrefix}: PrnServiceClient - UpdatePrnStatus: An error occurred updating prns statuses {UpdatePrns}", _logPrefix, JsonConvert.SerializeObject(updatePrns));
            throw;
        }
    }

    public async Task<ObligationModel> GetObligationCalculationByYearAsync(int year)
    {
        try
        {
            var orgId = await ConfigureHttpClientAsync();
            logger.LogInformation("{_logPrefix}: PrnServiceClient - GetObligationCalculationByYearAsync: calling endpoint '{ObligationCalculationUrl}/{Year}' for organisation {OrgId}", _logPrefix, ObligationCalculationUrl, year, orgId);

            var response = await httpClient.GetAsync($"{ObligationCalculationUrl}/{year}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            logger.LogInformation("{_logPrefix}: PrnServiceClient - GetObligationCalculationByYearAsync: response from endpoint {Response}", _logPrefix, content);
            return JsonConvert.DeserializeObject<ObligationModel>(content);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "{_logPrefix}: PrnServiceClient - GetObligationCalculationByYearAsync: An error occurred retrievig obligation calculations for organisation {ObligationCalculationUrl}/{Year}", _logPrefix, ObligationCalculationUrl, year);
            throw;
        }
    }

    private async Task<string> ConfigureHttpClientAsync()
    {
        var userId = httpContextAccessor.HttpContext.User.UserId();

        try
        {
            var complianceSchemeId = GetComplianceSchemeIdAsync();
            var organisationId = complianceSchemeId ?? (await accountServiceClient.GetUserAccount(userId)).User.Organisations[0].Id;

            httpClient.DefaultRequestHeaders.AddIfNotExists("X-EPR-ORGANISATION", organisationId.ToString());
            httpClient.DefaultRequestHeaders.AddIfNotExists("X-EPR-USER", userId.ToString());

            return organisationId.ToString();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "{_logPrefix}: PrnServiceClient - ConfigureHttpClientAsync: Error getting user accounts with id {UserId}", _logPrefix, userId);
            throw;
        }
    }

    private Guid? GetComplianceSchemeIdAsync()
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null || !context.Items.TryGetValue(ComplianceScheme.ComplianceSchemeId, out var value))
        {
            return null;
        }

        if (value is Guid complianceSchemeId)
        {
            return complianceSchemeId;
        }

        if (value is string complianceSchemeIdString && Guid.TryParse(complianceSchemeIdString, out var parsedComplianceSchemeId))
        {
            return parsedComplianceSchemeId;
        }

        return null;
    }
}

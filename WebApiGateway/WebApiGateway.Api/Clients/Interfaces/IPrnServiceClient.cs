﻿using WebApiGateway.Core.Models.Pagination;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.Api.Clients.Interfaces;

public interface IPrnServiceClient
{
    Task<List<PrnModel>> GetAllPrnsForOrganisation();

    Task<ObligationModel> GetObligationCalculationByYearAsync(int year);

    Task<PrnModel> GetPrnById(Guid id);

    Task UpdatePrnStatus(List<UpdatePrnStatus> updatePrns);

    Task<PaginatedResponse<PrnModel>> GetSearchPrns(PaginatedRequest request);
}
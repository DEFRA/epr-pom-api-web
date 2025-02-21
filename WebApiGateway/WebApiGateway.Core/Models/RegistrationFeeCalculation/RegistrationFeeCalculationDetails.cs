﻿namespace WebApiGateway.Core.Models.RegistrationFeeCalculation;

public class RegistrationFeeCalculationDetails
{
    public string OrganisationId { get; set; } = string.Empty;

    public int NumberOfSubsidiaries { get; set; }

    public int NumberOfSubsidiariesBeingOnlineMarketPlace { get; set; }

    public string OrganisationSize { get; set; } = string.Empty;

    public bool IsOnlineMarketplace { get; set; }

    public int NationId { get; set; }
}
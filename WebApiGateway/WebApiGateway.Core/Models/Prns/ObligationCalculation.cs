using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.Prns;

[ExcludeFromCodeCoverage]
public class ObligationCalculation
{
    public int OrganisationId { get; set; }

    public string MaterialName { get; set; }

    public int MaterialObligationValue { get; set; }

    public int Year { get; set; }
}
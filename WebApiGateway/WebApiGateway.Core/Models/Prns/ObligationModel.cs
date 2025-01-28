using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.Prns;

[ExcludeFromCodeCoverage]
public class ObligationModel
{
    public int NumberOfPrnsAwaitingAcceptance { get; set; }

    public List<ObligationData> ObligationData { get; set; } = new List<ObligationData>();
}

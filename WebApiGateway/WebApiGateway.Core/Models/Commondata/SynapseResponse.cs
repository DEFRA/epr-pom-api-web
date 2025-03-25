using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.Commondata;

[ExcludeFromCodeCoverage]
public class SynapseResponse
{
    public string OrganisationId { get; set; } = string.Empty;

    public bool IsFileSynced { get; set; }
}
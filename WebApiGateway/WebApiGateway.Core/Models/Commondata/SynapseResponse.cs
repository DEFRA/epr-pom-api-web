using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.Commondata;

[ExcludeFromCodeCoverage]
public class SynapseResponse
{
    public bool IsFileSynced { get; set; }
    
    public bool IsResubmissionDataSynced { get; set; }
}
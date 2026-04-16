using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.Commondata;

[ExcludeFromCodeCoverage]
public class SynapseResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether a flag that reflects when the file has been successfully processed by the "pip_get_meta_data_and_process_files" child pipeline of "pip_wrapper".
    /// </summary>
    /// <value>
    /// Boolean: True or False.
    /// </value>
    public bool IsFileSynced { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether a second flag that will reflect when the related resubmission data has been processed by the "pip_recyclers" child pipeline of "pip_wrapper".
    /// </summary>
    /// /// <value>
    /// Boolean: True or False.
    /// </value>
    public bool IsResubmissionDataSynced { get; set; }
}
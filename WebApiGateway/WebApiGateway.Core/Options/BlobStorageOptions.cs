namespace WebApiGateway.Core.Options;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class BlobStorageOptions
{
    public const string Section = "BlobStorage";

    [Required]
    public string ConnectionString { get; set; }

    [Required]
    public string SubsidiariesContainerName { get; set; }

    [Required]
    public string SubsidiariesFileUploadTemplateFileName { get; set; }
}

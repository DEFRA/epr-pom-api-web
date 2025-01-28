using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.Submission;

[ExcludeFromCodeCoverage]
public class UploadedPomFileInformation
{
    public string FileName { get; set; }

    public DateTime FileUploadDateTime { get; set; }

    public Guid UploadedBy { get; set; }

    public Guid FileId { get; set; }
}
namespace WebApiGateway.Core.Models.Submission;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class UploadedPomFileInformation
{
    public string FileName { get; set; }

    public DateTime FileUploadDateTime { get; set; }

    public Guid UploadedBy { get; set; }

    public Guid FileId { get; set; }
}
namespace WebApiGateway.Core.Models.Submission;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class SubmittedPomFileInformation
{
    public string FileName { get; set; }

    public Guid FileId { get; set; }

    public DateTime SubmittedDateTime { get; set; }

    public Guid SubmittedBy { get; set; }
}
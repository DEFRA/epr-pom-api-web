namespace WebApiGateway.Core.Models.Events;

using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

[ExcludeFromCodeCoverage]
public class FileDownloadCheckEvent : AbstractEvent
{
    public override EventType Type => EventType.FileDownloadCheck;

    public string ContentScan { get; set; }

    public Guid FileId { get; set; }

    public string FileName { get; set; }

    public string BlobName { get; set; }

    public Guid SubmissionId { get; set; }

    public SubmissionType SubmissionType { get; set; }
}

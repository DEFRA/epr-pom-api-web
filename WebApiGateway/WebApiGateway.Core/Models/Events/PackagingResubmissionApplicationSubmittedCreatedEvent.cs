using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Events;

[ExcludeFromCodeCoverage]
public class PackagingResubmissionApplicationSubmittedCreatedEvent : AbstractEvent
{
    public override EventType Type => EventType.PackagingResubmissionApplicationSubmitted;

    public Guid? FileId { get; set; }

    public bool? IsResubmitted { get; set; }

    public string? SubmittedBy { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public string? Comments { get; set; }
}

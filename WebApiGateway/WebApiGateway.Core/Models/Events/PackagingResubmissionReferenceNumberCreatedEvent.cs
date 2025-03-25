using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.Events;

namespace EPR.SubmissionMicroservice.Data.Entities.SubmissionEvent;

public class PackagingResubmissionReferenceNumberCreatedEvent : AbstractEvent
{
    public override EventType Type => EventType.PackagingResubmissionReferenceNumberCreated;

    public string? PackagingResubmissionReferenceNumber { get; set; }
}
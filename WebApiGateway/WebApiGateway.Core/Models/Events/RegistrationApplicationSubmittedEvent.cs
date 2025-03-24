using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Events;

[ExcludeFromCodeCoverage]
public class RegistrationApplicationSubmittedEvent : AbstractEvent
{
    public override EventType Type => EventType.RegistrationApplicationSubmitted;

    public string? Comments { get; set; }

    public string? ApplicationReferenceNumber { get; set; }

    public Guid? ComplianceSchemeId { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public bool? IsResubmission { get; set; }
}
using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Events;

[ExcludeFromCodeCoverage]
public class RegistrationFeePaymentEvent : AbstractEvent
{
    public override EventType Type => EventType.RegistrationFeePayment;

    public string? ApplicationReferenceNumber { get; set; }

    public Guid? ComplianceSchemeId { get; set; }

    public string PaymentMethod { get; set; }

    public string PaymentStatus { get; set; }

    public string PaidAmount { get; set; }
}
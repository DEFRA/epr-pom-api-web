using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Events;

[ExcludeFromCodeCoverage]
public class PackagingDataResubmissionFeePaymentEvent : AbstractEvent
{
    public override EventType Type => EventType.PackagingDataResubmissionFeePayment;

    public Guid? FileId { get; set; }

    public string? ReferenceNumber { get; set; }

    public string PaymentMethod { get; set; }

    public string PaymentStatus { get; set; }

    public string PaidAmount { get; set; }
}

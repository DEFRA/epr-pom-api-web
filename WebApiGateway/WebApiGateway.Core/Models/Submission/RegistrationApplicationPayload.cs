using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Submission;

[ExcludeFromCodeCoverage]
public class RegistrationApplicationPayload
{
    public string? ApplicationReferenceNumber { get; set; }

    public Guid? ComplianceSchemeId { get; set; }

    public string PaymentMethod { get; set; }

    public string PaymentStatus { get; set; }

    public string PaidAmount { get; set; }

    public string? Comments { get; set; }

    public SubmissionType SubmissionType { get; set; }

    public bool? IsResubmission { get; set; }

    public string? RegistrationJourney { get; set; }
}
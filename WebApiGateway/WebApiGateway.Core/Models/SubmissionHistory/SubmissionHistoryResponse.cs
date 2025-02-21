using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.SubmissionHistory;

[ExcludeFromCodeCoverage]
public class SubmissionHistoryResponse
{
    public Guid SubmissionId { get; set; }

    public string FileName { get; set; }

    public string UserName { get; set; }

    public DateTime SubmissionDate { get; set; }

    public string Status { get; set; }

    public Guid FileId { get; set; }

    public DateTime DateofLatestStatusChange { get; set; }

    public string ApplicationReferenceNumber { get; set; }

    public string Comments { get; set; }

    public DateTime? RegistrationApplicationSubmissionDate { get; set; }

    public string PaymentMethod { get; set; }

    public string PaymentStatus { get; set; }

    public string PaidAmount { get; set; }
}
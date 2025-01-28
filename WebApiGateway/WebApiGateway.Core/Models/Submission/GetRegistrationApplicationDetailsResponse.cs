using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Models.ComplianceSchemeDetails;
using WebApiGateway.Core.Models.ProducerDetails;

namespace WebApiGateway.Core.Models.Submission;

[ExcludeFromCodeCoverage]
public class GetRegistrationApplicationDetailsResponse
{
    public enum ApplicationStatusType
    {
        NotStarted,
        FileUploaded,
        SubmittedAndHasRecentFileUpload,
        SubmittedToRegulator,
        AcceptedByRegulator,
        RejectedByRegulator,
        ApprovedByRegulator,
        CancelledByRegulator,
        QueriedByRegulator
    }

    public Guid? SubmissionId { get; set; }

    public bool IsSubmitted { get; set; }

    public string? ApplicationReferenceNumber { get; set; } = string.Empty;

    public LastSubmittedFileDetails? LastSubmittedFile { get; set; }

    public string? RegistrationFeePaymentMethod { get; set; }

    public DateTime? RegistrationApplicationSubmittedDate { get; set; }

    public string? RegistrationApplicationSubmittedComment { get; set; }

    public bool RegistrationApplicationSubmitted => RegistrationApplicationSubmittedDate is not null;

    public ApplicationStatusType ApplicationStatus { get; set; }

    public GetProducerDetailsResponse? ProducerDetails { get; set; }

    public List<GetComplianceSchemeMemberDetailsResponse>? CsoMemberDetails { get; set; }

    public class LastSubmittedFileDetails
    {
        public Guid? FileId { get; set; }

        public string? SubmittedByName { get; set; } = string.Empty;

        public DateTime? SubmittedDateTime { get; set; }
    }
}
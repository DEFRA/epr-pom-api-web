using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Models.Commondata;

namespace EPR.SubmissionMicroservice.Application.Features.Queries.Common;

[ExcludeFromCodeCoverage]
public class PackagingResubmissionApplicationDetails
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

    public bool? IsResubmitted { get; set; }

    public bool? IsResubmissionFeeViewed { get; set; }

    public string? ApplicationReferenceNumber { get; set; } = string.Empty;

    public LastSubmittedFileDetails? LastSubmittedFile { get; set; }

    public string? ResubmissionFeePaymentMethod { get; set; }

    public DateTime? ResubmissionApplicationSubmittedDate { get; set; }

    public string? ResubmissionApplicationSubmittedComment { get; set; }

    public bool ResubmissionApplicationSubmitted => ResubmissionApplicationSubmittedDate is not null;

    public ApplicationStatusType ApplicationStatus { get; set; }

    public string? ResubmissionReferenceNumber { get; set; }

    public SynapseResponse SynapseResponse { get; set; } = new();

    public class LastSubmittedFileDetails
    {
      public Guid? FileId { get; set; }

      public string? SubmittedByName { get; set; } = string.Empty;

      public DateTime? SubmittedDateTime { get; set; }
    }
}
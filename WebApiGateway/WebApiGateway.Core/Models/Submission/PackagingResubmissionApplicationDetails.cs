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

    /// <summary>
    /// SUB-345: Gets or sets the most recent resubmission cycle the regulator has ruled on, or null if there is none.
    /// </summary>
    /// <remarks>
    /// Every field above describes the cycle that is open now, so the submission API stops reporting all of
    /// them at the decision that closed a cycle. Forwarded separately, this is what lets the frontend tell a
    /// completed resubmission from one that was never started.
    /// </remarks>
    /// <value>
    /// The most recent resubmission cycle the regulator has ruled on, or null if there is none.
    /// </value>
    public CompletedResubmissionDetails? LastCompletedResubmission { get; set; }

    /// <summary>
    /// SUB-345: Gets or sets a value indicating whether the cycle every field above describes has been closed
    /// by a regulator decision, with nothing having opened a later one.
    /// </summary>
    /// <remarks>
    /// ApplicationReferenceNumber is reported on every path so the cycle keeps its identity, which leaves an
    /// empty one meaning only "the very first cycle". Forwarded, this is what tells the frontend the number it
    /// has belongs to a finished cycle and the next resubmission needs one of its own.
    /// </remarks>
    /// <value>
    /// True when the reported cycle has been ruled on and nothing has replaced it; otherwise false.
    /// </value>
    public bool IsResubmissionCycleClosed { get; set; }

    public SynapseResponse SynapseResponse { get; set; } = new();

    public class LastSubmittedFileDetails
    {
      public Guid? FileId { get; set; }

      public string? SubmittedByName { get; set; } = string.Empty;

      public DateTime? SubmittedDateTime { get; set; }
    }

    public class CompletedResubmissionDetails
    {
      public string? ApplicationReferenceNumber { get; set; }

      public string? ResubmissionReferenceNumber { get; set; }

      public DateTime? DeclarationDate { get; set; }

      public string? DeclarationComment { get; set; }

      public string? DeclaredByName { get; set; }

      public bool? IsResubmissionFeeViewed { get; set; }

      public string? ResubmissionFeePaymentMethod { get; set; }

      public string? Decision { get; set; }

      public string? RegulatorComments { get; set; }

      public DateTime? DecisionDate { get; set; }

      public string? FileName { get; set; }

      public LastSubmittedFileDetails? SubmittedFile { get; set; }
    }
}
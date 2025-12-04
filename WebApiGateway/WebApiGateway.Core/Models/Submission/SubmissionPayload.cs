using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.Submission;

[ExcludeFromCodeCoverage]
public class SubmissionPayload
{
    public Guid FileId { get; set; }

    public string? SubmittedBy { get; set; }

    public string? AppReferenceNumber { get; set; }

    public bool? IsResubmission { get; set; }

    public string? RegistrationJourney { get; set; }
}
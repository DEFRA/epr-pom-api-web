using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Submission;

[ExcludeFromCodeCoverage]
public class PomSubmission : AbstractSubmission
{
    public override SubmissionType SubmissionType => SubmissionType.Producer;

    public string PomFileName { get; set; }

    public DateTime? PomFileUploadDateTime { get; set; }

    public bool PomDataComplete { get; set; }

    public string? AppReferenceNumber { get; set; }

    public bool? IsResubmissionInProgress { get; set; }

    public bool? IsResubmissionComplete { get; set; }

    public UploadedPomFileInformation? LastUploadedValidFile { get; set; }

    public SubmittedPomFileInformation? LastSubmittedFile { get; set; }
}
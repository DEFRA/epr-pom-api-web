namespace WebApiGateway.Core.Models.Submission;

using System.Diagnostics.CodeAnalysis;
using Enumeration;

[ExcludeFromCodeCoverage]
public class PomSubmission : AbstractSubmission
{
    public override SubmissionType SubmissionType => SubmissionType.Producer;

    public string PomFileName { get; set; }

    public DateTime? PomFileUploadDateTime { get; set; }

    public bool PomDataComplete { get; set; }

    public UploadedPomFileInformation? LastUploadedValidFile { get; set; }

    public SubmittedPomFileInformation? LastSubmittedFile { get; set; }
}
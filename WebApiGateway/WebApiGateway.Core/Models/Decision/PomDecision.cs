namespace WebApiGateway.Core.Models.Decision;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class PomDecision
{
    public Guid Id { get; set; }

    public Guid SubmissionId { get; set; }

    public string Type { get; }

    public Guid FileId { get; set; }

    public Guid SubmissionEventId { get; set; }

    public string Decision { get; set; }

    public string? Comments { get; set; }

    public bool IsResubmissionRequired { get; set; }

    public DateTime Created { get; set; }
}
using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.Decision;

[ExcludeFromCodeCoverage]
public class RegulatorDecision
{
    public Guid Id { get; set; }

    public Guid SubmissionId { get; set; }

    public string Type { get; } = string.Empty;

    public Guid FileId { get; set; }

    public Guid SubmissionEventId { get; set; }

    public string Decision { get; set; }

    public string? Comments { get; set; }

    public bool IsResubmissionRequired { get; set; }

    public DateTime Created { get; set; }
}
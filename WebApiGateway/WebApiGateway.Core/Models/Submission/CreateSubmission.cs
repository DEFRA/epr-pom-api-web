using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Submission;

[ExcludeFromCodeCoverage]
public class CreateSubmission
{
    public Guid Id { get; set; }

    public DataSourceType DataSourceType { get; set; }

    public SubmissionType SubmissionType { get; set; }

    public string SubmissionPeriod { get; set; }

    public Guid? ComplianceSchemeId { get; set; }

    public string? AppReferenceNumber { get; set; }
}
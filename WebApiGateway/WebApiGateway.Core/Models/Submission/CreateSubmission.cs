namespace WebApiGateway.Core.Models.Submission;

using System.Diagnostics.CodeAnalysis;
using Enumeration;

[ExcludeFromCodeCoverage]
public class CreateSubmission
{
    public Guid Id { get; set; }

    public DataSourceType DataSourceType { get; set; }

    public SubmissionType SubmissionType { get; set; }

    public string SubmissionPeriod { get; set; }

    public Guid? ComplianceSchemeId { get; set; }
}
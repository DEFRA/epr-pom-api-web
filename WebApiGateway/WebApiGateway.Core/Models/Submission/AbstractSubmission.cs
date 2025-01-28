using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using WebApiGateway.Core.Converters;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Submission;

[ExcludeFromCodeCoverage]
[JsonConverter(typeof(AbstractSubmissionConverter))]
public abstract class AbstractSubmission
{
    public Guid Id { get; set; }

    public DataSourceType DataSourceType { get; set; }

    public abstract SubmissionType SubmissionType { get; }

    public string SubmissionPeriod { get; set; }

    public Guid OrganisationId { get; set; }

    public Guid UserId { get; set; }

    public DateTime Created { get; set; }

    public bool ValidationPass { get; set; }

    public List<string> Errors { get; set; } = new();

    public bool IsSubmitted { get; set; }

    public Guid? ComplianceSchemeId { get; set; }

    public bool HasValidFile { get; set; }

    public bool HasWarnings { get; set; }

    public string? AppReferenceNumber { get; set; }
}
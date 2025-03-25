using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.PackagingResubmissionApplication;

[ExcludeFromCodeCoverage]
public class PackagingResubmissionMemberRequest
{
    public Guid? FileId { get; set; }

    public string SubmissionYears { get; set; }

    public string SubmissionPeriods { get; set; }

    public int PageSize { get; set; }
}

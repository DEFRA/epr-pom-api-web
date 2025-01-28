using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Submission;

[ExcludeFromCodeCoverage]
public class SubsidiarySubmission : AbstractSubmission
{
    public override SubmissionType SubmissionType => SubmissionType.Subsidiary;

    public bool SubsidiaryDataComplete { get; set; }
}

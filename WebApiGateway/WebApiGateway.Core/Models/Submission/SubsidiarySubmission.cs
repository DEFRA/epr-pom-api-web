namespace WebApiGateway.Core.Models.Submission;

using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

[ExcludeFromCodeCoverage]
public class SubsidiarySubmission : AbstractSubmission
{
    public override SubmissionType SubmissionType => SubmissionType.Subsidiary;

    public bool SubsidiaryDataComplete { get; set; }
}

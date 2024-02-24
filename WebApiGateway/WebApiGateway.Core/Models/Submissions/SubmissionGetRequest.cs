using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Submissions
{
    [ExcludeFromCodeCoverage]
    public class SubmissionGetRequest
    {
        public Guid? ComplianceSchemeId { get; set; }

        public int? Year { get; set; }

        public SubmissionType Type { get; set; }
    }
}

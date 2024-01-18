using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Options;

[ExcludeFromCodeCoverage]
public class DecisionApiOptions
{
    public const string Section = "SubmissionStatusApi";

    public string BaseUrl { get; set; }
}
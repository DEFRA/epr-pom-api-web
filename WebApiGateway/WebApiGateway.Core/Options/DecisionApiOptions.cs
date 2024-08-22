namespace WebApiGateway.Core.Options;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class DecisionApiOptions
{
    public const string Section = "SubmissionStatusApi";

    public string BaseUrl { get; set; }
}
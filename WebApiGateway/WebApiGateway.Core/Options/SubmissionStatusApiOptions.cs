using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Options;

[ExcludeFromCodeCoverage]
public class SubmissionStatusApiOptions
{
    public const string Section = "SubmissionStatusApi";

    public string BaseUrl { get; set; } = string.Empty;
}
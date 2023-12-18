namespace WebApiGateway.Core.Options;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class SubmissionStatusApiOptions
{
    public const string Section = "SubmissionStatusApi";

    public string BaseUrl { get; set; }
}
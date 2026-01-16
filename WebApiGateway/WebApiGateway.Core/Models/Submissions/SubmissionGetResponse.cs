using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace WebApiGateway.Core.Models.Submissions;

[ExcludeFromCodeCoverage]
public class SubmissionGetResponse
{
    [JsonPropertyName("SubmissionId")]
    public Guid SubmissionId { get; set; }

    [JsonPropertyName("SubmissionPeriod")]
    public string SubmissionPeriod { get; set; }

    [JsonPropertyName("Year")]
    public int Year { get; set; }

    public string? RegistrationJourney { get; set; }
}
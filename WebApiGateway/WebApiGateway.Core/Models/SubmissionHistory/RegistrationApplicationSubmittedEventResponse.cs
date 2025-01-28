using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace WebApiGateway.Core.Models.SubmissionHistory;

[ExcludeFromCodeCoverage]
public class RegistrationApplicationSubmittedEventResponse
{
    [JsonPropertyName("submissionId")]
    public Guid SubmissionId { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("comments")]
    public string Comments { get; set; }

    [JsonPropertyName("applicationReferenceNumber")]
    public string ApplicationReferenceNumber { get; set; }

    [JsonPropertyName("submissionDate")]
    public DateTime? SubmissionDate { get; set; }
}
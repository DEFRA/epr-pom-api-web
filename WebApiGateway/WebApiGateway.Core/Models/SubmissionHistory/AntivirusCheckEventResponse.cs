using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace WebApiGateway.Core.Models.SubmissionHistory;

[ExcludeFromCodeCoverage]
public class AntivirusCheckEventResponse
{
    [JsonPropertyName("submissionId")]
    public Guid SubmissionId { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("fileId")]
    public Guid FileId { get; set; }

    [JsonPropertyName("fileName")]
    public string FileName { get; set; }
}
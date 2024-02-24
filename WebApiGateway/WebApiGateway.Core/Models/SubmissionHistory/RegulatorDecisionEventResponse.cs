using System.Text.Json.Serialization;

namespace WebApiGateway.Core.Models.SubmissionHistory
{
    public class RegulatorDecisionEventResponse
    {
        [JsonPropertyName("decision")]
        public string Decision { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

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
}
using System.Text.Json.Serialization;

namespace WebApiGateway.Core.Models.SubmissionHistory
{
    public class SubmissionHistoryEventsResponse
    {
        [JsonPropertyName("submittedEvents")]
        public List<SubmittedEventResponse> SubmittedEvents { get; set; }

        [JsonPropertyName("regulatorDecisionEvents")]
        public List<RegulatorDecisionEventResponse> RegulatorDecisionEvents { get; set; }

        [JsonPropertyName("antivirusCheckEvents")]
        public List<AntivirusCheckEventResponse> AntivirusCheckEvents { get; set; }
    }
}

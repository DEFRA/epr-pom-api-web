using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace WebApiGateway.Core.Models.SubmissionHistory;

[ExcludeFromCodeCoverage]
public class RegistrationFeePaymentEventResponse
{
    [JsonPropertyName("submissionId")]
    public Guid SubmissionId { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("applicationReferenceNumber")]
    public string ApplicationReferenceNumber { get; set; }

    [JsonPropertyName("PaymentMethod")]
    public string PaymentMethod { get; set; }

    [JsonPropertyName("PaymentStatus")]
    public string PaymentStatus { get; set; }

    [JsonPropertyName("PaidAmount")]
    public string PaidAmount { get; set; }
}
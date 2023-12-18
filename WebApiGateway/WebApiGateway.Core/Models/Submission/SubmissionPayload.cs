namespace WebApiGateway.Core.Models.Submission;

public class SubmissionPayload
{
    public Guid FileId { get; set; }

    public string? SubmittedBy { get; set; }
}
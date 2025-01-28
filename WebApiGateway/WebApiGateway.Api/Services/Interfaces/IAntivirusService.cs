namespace WebApiGateway.Api.Services.Interfaces;

using Core.Enumeration;

public interface IAntivirusService
{
    Task SendFileAsync(SubmissionType submissionType, Guid fileId, string fileName, Stream fileStream);

    Task<HttpResponseMessage> SendFileAndScanAsync(SubmissionType submissionType, Guid fileId, string fileName, MemoryStream fileStream);
}
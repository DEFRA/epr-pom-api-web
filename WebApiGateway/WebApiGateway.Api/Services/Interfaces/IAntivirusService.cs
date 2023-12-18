namespace WebApiGateway.Api.Services.Interfaces;

using Core.Enumeration;

public interface IAntivirusService
{
    Task SendFileAsync(SubmissionType submissionType, Guid fileId, string fileName, Stream fileStream);
}
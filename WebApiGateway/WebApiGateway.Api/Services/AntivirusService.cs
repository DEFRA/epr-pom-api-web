using Microsoft.Extensions.Options;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.Antivirus;
using WebApiGateway.Core.Options;

namespace WebApiGateway.Api.Services;

public class AntivirusService(
    IAntivirusClient antivirusClient,
    IHttpContextAccessor httpContextAccessor,
    IOptions<AntivirusApiOptions> antivirusApiOptions)
    : IAntivirusService
{
    private readonly AntivirusApiOptions _antivirusApiOptions = antivirusApiOptions.Value;

    public async Task SendFileAsync(SubmissionType submissionType, Guid fileId, string fileName, Stream fileStream)
    {
        var fileDetails = new FileDetails
        {
            Key = fileId,
            Extension = Path.GetExtension(fileName),
            FileName = Path.GetFileNameWithoutExtension(fileName),
            Collection = GetCollectionName(submissionType.GetDisplayName()),
            UserId = httpContextAccessor.HttpContext.User.UserId(),
            UserEmail = httpContextAccessor.HttpContext.User.Email()
        };

        await antivirusClient.SendFileAsync(fileDetails, fileName, fileStream);
    }

    public async Task<HttpResponseMessage> SendFileAndScanAsync(SubmissionType submissionType, Guid fileId, string fileName, MemoryStream fileStream)
    {
        var fileDetails = new FileDetails
        {
            Key = fileId,
            Extension = Path.GetExtension(fileName),
            FileName = Path.GetFileNameWithoutExtension(fileName),
            Collection = GetCollectionName(submissionType.GetDisplayName()),
            UserId = httpContextAccessor.HttpContext.User.UserId(),
            UserEmail = httpContextAccessor.HttpContext.User.Email()
        };

        return await antivirusClient.VirusScanFileAsync(fileDetails, fileName, fileStream);
    }

    private string GetCollectionName(string submissionType)
    {
        var suffix = _antivirusApiOptions?.CollectionSuffix;
        return suffix is null ? submissionType : submissionType + suffix;
    }
}
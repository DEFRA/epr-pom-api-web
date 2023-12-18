namespace WebApiGateway.Api.Services;

using Clients.Interfaces;
using Core.Enumeration;
using Core.Models.Antivirus;
using Core.Options;
using Extensions;
using Interfaces;
using Microsoft.Extensions.Options;

public class AntivirusService : IAntivirusService
{
    private readonly IAntivirusClient _antivirusClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AntivirusApiOptions _antivirusApiOptions;

    public AntivirusService(IAntivirusClient antivirusClient, IHttpContextAccessor httpContextAccessor, IOptions<AntivirusApiOptions> antivirusApiOptions)
    {
        _antivirusClient = antivirusClient;
        _httpContextAccessor = httpContextAccessor;
        _antivirusApiOptions = antivirusApiOptions.Value;
    }

    public async Task SendFileAsync(SubmissionType submissionType, Guid fileId, string fileName, Stream fileStream)
    {
        var fileDetails = new FileDetails
        {
            Key = fileId,
            Extension = Path.GetExtension(fileName),
            FileName = Path.GetFileNameWithoutExtension(fileName),
            Collection = GetCollectionName(submissionType.GetDisplayName()),
            UserId = _httpContextAccessor.HttpContext.User.UserId(),
            UserEmail = _httpContextAccessor.HttpContext.User.Email()
        };

        await _antivirusClient.SendFileAsync(fileDetails, fileName, fileStream);
    }

    private string GetCollectionName(string submissionType)
    {
        var suffix = _antivirusApiOptions?.CollectionSuffix;
        return suffix is null ? submissionType : submissionType + suffix;
    }
}
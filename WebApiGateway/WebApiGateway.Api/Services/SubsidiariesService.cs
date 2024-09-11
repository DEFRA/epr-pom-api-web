namespace WebApiGateway.Api.Services;

using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Subsidiaries;
using WebApiGateway.Core.Options;

public class SubsidiariesService : ISubsidiariesService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobStorageOptions _blobStorageOptions;
    private readonly ILogger<SubsidiariesService> _logger;

    public SubsidiariesService(BlobServiceClient blobServiceClient, IOptions<BlobStorageOptions> blobStorageOptions, ILogger<SubsidiariesService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _blobStorageOptions = blobStorageOptions.Value;
        _logger = logger;
    }

    public async Task<GetFileUploadTemplateResponse> GetFileUploadTemplateAsync()
    {
        var blobCleint = _blobServiceClient.GetBlobContainerClient(_blobStorageOptions.SubsidiariesContainerName)
            .GetBlobClient(_blobStorageOptions.SubsidiariesFileUploadTemplateFileName);

        try
        {
            var blobProperties = await blobCleint.GetPropertiesAsync();
            var blobDownloadResult = await blobCleint.DownloadContentAsync();

            return new GetFileUploadTemplateResponse
            {
                Name = blobCleint.Name,
                ContentType = blobProperties.Value.ContentType,
                Content = blobDownloadResult.Value.Content.ToStream(),
            };
        }
        catch (RequestFailedException ex)
        {
            if (ex.Status == 404)
            {
                _logger.LogError(ex, "Subsidiaries file upload template download failed.");

                return null;
            }

            throw;
        }
    }
}

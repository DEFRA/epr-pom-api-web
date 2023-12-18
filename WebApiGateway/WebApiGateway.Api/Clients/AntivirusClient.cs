namespace WebApiGateway.Api.Clients;

using Core.Models.Antivirus;
using Interfaces;
using Newtonsoft.Json;

public class AntivirusClient : IAntivirusClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AntivirusClient> _logger;

    public AntivirusClient(HttpClient httpClient, ILogger<AntivirusClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendFileAsync(FileDetails fileDetails, string fileName, Stream fileStream)
    {
        try
        {
            var formContent = new MultipartFormDataContent
            {
                { new StringContent(JsonConvert.SerializeObject(fileDetails)), nameof(fileDetails) },
                { new StreamContent(fileStream), nameof(fileStream), fileName }
            };

            var boundary = formContent.Headers.ContentType.Parameters.First(o => o.Name == "boundary");
            boundary.Value = boundary.Value.Replace("\"", string.Empty);

            var response = await _httpClient.PutAsync($"files/stream/{fileDetails.Collection}/{fileDetails.Key}", formContent);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error sending file to antivirus api");
            throw;
        }
    }
}
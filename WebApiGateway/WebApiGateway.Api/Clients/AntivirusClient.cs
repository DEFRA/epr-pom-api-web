using System.Text;
using Newtonsoft.Json;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Core.Models.Antivirus;

namespace WebApiGateway.Api.Clients;

public class AntivirusClient(
    HttpClient httpClient,
    ILogger<AntivirusClient> logger)
    : IAntivirusClient
{
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

            var response = await httpClient.PutAsync($"files/stream/{fileDetails.Collection}/{fileDetails.Key}", formContent);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error sending file to antivirus api");
            throw;
        }
    }

    public async Task<HttpResponseMessage> VirusScanFileAsync(FileDetails fileDetails, string fileName, MemoryStream fileStream)
    {
        try
        {
            fileDetails.Content = Convert.ToBase64String(fileStream.ToArray());

            var jsonRequest = System.Text.Json.JsonSerializer.Serialize(fileDetails);
            var stringContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync($"SyncAV/{fileDetails.Collection}/{fileDetails.Key}", stringContent);
            response.EnsureSuccessStatusCode();

            return response;
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error sending file to antivirus api");
            throw;
        }
    }
}
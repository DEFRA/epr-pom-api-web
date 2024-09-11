namespace WebApiGateway.Core.Models.Subsidiaries;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class GetFileUploadTemplateResponse
{
    public string Name { get; set; }

    public string ContentType { get; set; }

    public Stream Content { get; set; }
}
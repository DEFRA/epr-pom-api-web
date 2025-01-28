using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.FileDownload;

[ExcludeFromCodeCoverage]
public class FileDownloadData
{
    public MemoryStream Stream { get; set; }

    public string AntiVirusResult { get; set; }
}

using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.Antivirus;

[ExcludeFromCodeCoverage]
public class FileDetails
{
    public string Service => "epr";

    public Guid Key { get; set; }

    public string Collection { get; set; }

    public string Extension { get; set; }

    public string FileName { get; set; }

    public Guid UserId { get; set; }

    public string UserEmail { get; set; }

    public string Content { get; set; }
}
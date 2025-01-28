using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Events;

[ExcludeFromCodeCoverage]
public class AntivirusCheckEvent : AbstractEvent
{
    public override EventType Type => EventType.AntivirusCheck;

    public Guid FileId { get; set; }

    public string FileName { get; set; }

    public FileType FileType { get; set; }

    public Guid? RegistrationSetId { get; set; }
}
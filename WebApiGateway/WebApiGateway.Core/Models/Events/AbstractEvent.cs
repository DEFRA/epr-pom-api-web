using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Events;

[ExcludeFromCodeCoverage]
public abstract class AbstractEvent
{
    public abstract EventType Type { get; }

    public string? BlobContainerName { get; set; }
}
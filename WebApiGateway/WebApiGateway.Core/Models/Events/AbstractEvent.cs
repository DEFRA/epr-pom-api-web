namespace WebApiGateway.Core.Models.Events;

using Enumeration;

public abstract class AbstractEvent
{
    public abstract EventType Type { get; }

    public string? BlobContainerName { get; set; }
}
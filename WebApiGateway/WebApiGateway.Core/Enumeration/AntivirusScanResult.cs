namespace WebApiGateway.Core.Enumeration;

public enum AntivirusScanResult
{
    AwaitingProcessing = 1,
    Success = 2,
    FileInaccessible = 3,
    Quarantined = 4,
    FailedToVirusScan = 5,
    Started = 6
}
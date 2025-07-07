using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Options;

[ExcludeFromCodeCoverage]
public class StorageAccountOptions
{
    public const string Section = "StorageAccount";

    public string ConnectionString { get; set; }

    public string PomContainer { get; set; } = string.Empty;

    public string RegistrationContainer { get; set; } = string.Empty;

    public string SubsidiaryContainer { get; set; } = string.Empty;

    public string AccreditationContainer { get; set; } = string.Empty;
}
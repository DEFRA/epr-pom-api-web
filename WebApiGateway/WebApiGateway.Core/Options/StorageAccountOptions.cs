namespace WebApiGateway.Core.Options;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class StorageAccountOptions
{
    public const string Section = "StorageAccount";

    public string PomContainer { get; set; }

    public string RegistrationContainer { get; set; }

    public string SubsidiaryContainer { get; set; }
}
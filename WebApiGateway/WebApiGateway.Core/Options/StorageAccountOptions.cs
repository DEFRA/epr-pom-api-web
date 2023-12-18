namespace WebApiGateway.Core.Options;

public class StorageAccountOptions
{
    public const string Section = "StorageAccount";

    public string PomContainer { get; set; }

    public string RegistrationContainer { get; set; }
}
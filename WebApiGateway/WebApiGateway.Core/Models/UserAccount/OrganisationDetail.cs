namespace WebApiGateway.Core.Models.UserAccount;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class OrganisationDetail
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string OrganisationRole { get; set; }

    public string OrganisationType { get; set; }

    public string ProducerType { get; set; }
}
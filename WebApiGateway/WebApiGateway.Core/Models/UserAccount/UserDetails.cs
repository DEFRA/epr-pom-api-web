using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.UserAccount;

[ExcludeFromCodeCoverage]
public class UserDetails
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string? RoleInOrganisation { get; set; }

    public string? EnrolmentStatus { get; set; }

    public string? ServiceRole { get; set; }

    public string? Service { get; set; }

    public int? ServiceRoleId { get; set; }

    public List<OrganisationDetail> Organisations { get; set; }
}
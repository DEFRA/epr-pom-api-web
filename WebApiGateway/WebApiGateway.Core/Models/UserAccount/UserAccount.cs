using System.Diagnostics.CodeAnalysis;

namespace WebApiGateway.Core.Models.UserAccount;

[ExcludeFromCodeCoverage]
public class UserAccount
{
    public UserDetails User { get; set; }
}
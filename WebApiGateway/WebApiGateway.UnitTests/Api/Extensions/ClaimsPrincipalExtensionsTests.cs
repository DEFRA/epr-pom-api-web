using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Extensions;

namespace WebApiGateway.UnitTests.Api.Extensions;

[TestClass]
public class ClaimsPrincipalExtensionsTests
{
    private const string UserEmail = "janedoe@here.com";
    private readonly Guid _userId = Guid.NewGuid();

    private Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private DefaultHttpContext _httpContext;

    [TestInitialize]
    public void TestInitialize()
    {
        _claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        _claimsPrincipalMock.Setup(x => x.Claims).Returns(GetClaims);
        _httpContext = new DefaultHttpContext { User = _claimsPrincipalMock.Object };
    }

    [TestMethod]
    public void UserId_ReturnsUserId()
    {
        // Arrange / Act
        var result = _httpContext.User.UserId();

        // Assert
        result.Should().Be(_userId);
    }

    [TestMethod]
    public void Email_ReturnsUserEmail()
    {
        // Arrange / Act
        var result = _httpContext.User.Email();

        // Assert
        result.Should().Be(UserEmail);
    }

    private List<Claim> GetClaims() => new()
    {
        new Claim(ClaimConstants.ObjectId, _userId.ToString()),
        new Claim("emails", UserEmail)
    };
}
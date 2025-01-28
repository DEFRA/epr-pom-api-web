using System.Net;
using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Core.Models.Decision;
using WebApiGateway.Core.Models.UserAccount;
using WebApiGateway.UnitTests.Support.Extensions;

namespace WebApiGateway.UnitTests.Api.Clients;

[TestClass]
public class DecisionClientTests
{
    private static readonly IFixture Fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly UserAccount _userAccount = Fixture.Create<UserAccount>();

    private Mock<ILogger<DecisionClient>> _loggerMock;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private DecisionClient _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _loggerMock = new Mock<ILogger<DecisionClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://example.com")
        };

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();

        claimsPrincipalMock.Setup(x => x.Claims).Returns(new List<Claim>
        {
            new(ClaimConstants.ObjectId, _userAccount.User.Id.ToString())
        });

        var accountServiceClientMock = new Mock<IAccountServiceClient>();
        accountServiceClientMock.Setup(x => x.GetUserAccount(_userAccount.User.Id)).ReturnsAsync(_userAccount);
        httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipalMock.Object);
        _systemUnderTest = new DecisionClient(httpClient, accountServiceClientMock.Object, httpContextAccessorMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetDecisionAsync_ReturnsOkHttpResponseMessage()
    {
        // Arrange
        var decision = new List<RegulatorDecision>();
        var expectedId = Guid.NewGuid();
        decision.Add(new RegulatorDecision { Id = expectedId, Decision = "Accepted", Created = DateTime.Now.AddHours(-2) });
        decision.Add(new RegulatorDecision { Id = Guid.NewGuid(), Decision = "Rejected", Created = DateTime.Now.AddDays(-2) });

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, decision.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetDecisionAsync("?key=value");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RegulatorDecision>();
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedId);
    }

    [TestMethod]
    [ExpectedException(typeof(HttpRequestException))]
    public async Task GetDecisionAsync_ReturnsErrorHttpResponseMessage()
    {
        // Arrange
        var decision = new List<RegulatorDecision>();
        var expectedId = Guid.NewGuid();
        decision.Add(new RegulatorDecision { Id = expectedId, Decision = "Accepted", Created = DateTime.Now.AddHours(-2) });
        decision.Add(new RegulatorDecision { Id = Guid.NewGuid(), Decision = "Rejected", Created = DateTime.Now.AddDays(-2) });

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, decision.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetDecisionAsync("?key=value");

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetDecisionAsync_ReturnsEmptyResultSetAndOkHttpResponseMessage()
    {
        // Arrange
        var decision = new List<RegulatorDecision>();

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, decision.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetDecisionAsync("?key=value");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RegulatorDecision>();
        result.Should().NotBeNull();
        result.Id.Should().Be(Guid.Empty);
    }
}
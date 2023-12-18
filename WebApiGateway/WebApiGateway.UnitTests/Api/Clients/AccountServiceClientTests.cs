namespace WebApiGateway.UnitTests.Api.Clients;

using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Support.Extensions;
using WebApiGateway.Api.Clients;
using WebApiGateway.Core.Models.UserAccount;

[TestClass]
public class AccountServiceClientTests
{
    private static readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly UserAccount _userAccount = _fixture.Create<UserAccount>();

    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private Mock<ILogger<AccountServiceClient>> _loggerMock;
    private AccountServiceClient _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _loggerMock = new Mock<ILogger<AccountServiceClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://example.com")
        };

        _systemUnderTest = new AccountServiceClient(httpClient, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetUserAccount_ReturnsUser_WhenHttpClientResponseIsOk()
    {
        // Arrange
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, _userAccount.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetUserAccount(_userAccount.User.Id);

        // Assert
        result.Should().BeEquivalentTo(_userAccount);

        var expectedMethod = HttpMethod.Get;
        var expectedRequestUri = new Uri($"https://example.com/users/user-organisations?userId={_userAccount.User.Id}");

        _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, Times.Once());
    }

    [TestMethod]
    public async Task GetUserAccount_LogsAndThrowsException_WhenHttpClientResponseIsNotFound()
    {
        // Arrange
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.NotFound, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.GetUserAccount(_userAccount.User.Id))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "An error occurred retrieving user by id: {userId}", _userAccount.User.Id));
    }
}
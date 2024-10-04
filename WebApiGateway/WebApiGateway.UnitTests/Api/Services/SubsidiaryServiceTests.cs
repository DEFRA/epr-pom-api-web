using System.Security.Claims;
using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StackExchange.Redis;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Models.Subsidiary;
using WebApiGateway.Core.Models.UserAccount;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class SubsidiaryServiceTests
{
    private static readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly UserAccount _userAccount = _fixture.Create<UserAccount>();

    private Mock<ILogger<SubsidiaryService>> _loggerMock;
    private Mock<IAccountServiceClient> _accountServiceClientMock;
    private Mock<IConnectionMultiplexer> _connectionMultiplexerMock;
    private Mock<IDatabase> _databaseMock;

    private SubsidiaryService _service;

    [TestInitialize]
    public void Setup()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();

        claimsPrincipalMock.Setup(x => x.Claims).Returns(new List<Claim>
        {
            new(ClaimConstants.ObjectId, _userAccount.User.Id.ToString())
        });

        _accountServiceClientMock = new Mock<IAccountServiceClient>();
        _accountServiceClientMock.Setup(x => x.GetUserAccount(_userAccount.User.Id)).ReturnsAsync(_userAccount);
        httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipalMock.Object);

        _loggerMock = new Mock<ILogger<SubsidiaryService>>();

        _databaseMock = new Mock<IDatabase>();
        _connectionMultiplexerMock = new Mock<IConnectionMultiplexer>();

        _connectionMultiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);

        _service = new SubsidiaryService(_accountServiceClientMock.Object, httpContextAccessorMock.Object, _connectionMultiplexerMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetNotificationErrorsAsync_EmptyKey_ReturnsEmptyResponse()
    {
        // Arrange
        var key = "emptyKey";

        _databaseMock.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _service.GetNotificationErrorsAsync(key);

        // Assert
        result.Should().NotBeNull();
        result.Errors.Should().BeNull();

        _loggerMock.VerifyLog(x => x.LogInformation("Redis empty errors response key: {Key}", key), Times.Once);
    }

    [TestMethod]
    public async Task GetNotificationErrorsAsync_ValidKey_ReturnsErrorsResponse()
    {
        // Arrange
        var key = "validKey";
        var errors = new UploadFileErrorResponse
        {
            Status = "Finished",
            Errors = new List<UploadFileErrorModel>
                {
                    new UploadFileErrorModel { FileLineNumber = 1, FileContent = "Content1", Message = "Message1", IsError = true, ErrorNumber = 6 },
                    new UploadFileErrorModel { FileLineNumber = 2, FileContent = "Content2", Message = "Message2", IsError = false, ErrorNumber = 9 }
                }
        };

        var json = JsonSerializer.Serialize(errors);
        _databaseMock.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)json);

        // Act
        var result = await _service.GetNotificationErrorsAsync(key);

        // Assert
        result.Should().NotBeNull();
        result.Errors.Count.Should().Be(2);
        result.Errors[0].FileLineNumber.Should().Be(1);
        result.Errors[0].FileContent.Should().Be("Content1");
        result.Errors[0].Message.Should().Be("Message1");
        result.Errors[0].ErrorNumber.Should().Be(6);
        result.Errors[0].IsError.Should().BeTrue();
        result.Errors[1].FileLineNumber.Should().Be(2);
        result.Errors[1].FileContent.Should().Be("Content2");
        result.Errors[1].Message.Should().Be("Message2");
        result.Errors[1].IsError.Should().BeFalse();
        result.Errors[1].ErrorNumber.Should().Be(9);
        result.Status.Should().Be("Finished");

        _loggerMock.VerifyLog(x => x.LogInformation("Redis errors response key: {Key} errors: {Value}", key, json), Times.Once);
    }

    [TestMethod]
    public async Task GetNotificationStatusAsync_WhenValueIsEmpty_ShouldReturnEmptyString()
    {
        // Arrange
        var key = "testKey";
        _databaseMock.Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>()))
                          .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _service.GetNotificationStatusAsync(key);

        // Assert
        result.Should().Be(string.Empty);
        _loggerMock.VerifyLog(x => x.LogInformation("Redis empty status response key: {Key}", key), Times.Once);
    }

    [TestMethod]
    public async Task GetNotificationStatusAsync_WhenValueIsNotEmpty_ShouldReturnValue()
    {
        // Arrange
        var key = "testKey";
        var redisValue = "SomeStatus";
        _databaseMock.Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>()))
                          .ReturnsAsync(redisValue);

        // Act
        var result = await _service.GetNotificationStatusAsync(key);

        // Assert
        result.Should().Be(redisValue);
        _loggerMock.VerifyLog(x => x.LogInformation("Redis status response key: {Key} value: {Value}", key, redisValue), Times.Once);
    }

    [TestMethod]
    public async Task GetNotificationStatusAsync_WhenExceptionThrown_ShouldLogError()
    {
        // Arrange
        var key = "testKey";
        _databaseMock.Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>()))
                          .ThrowsAsync(new RedisException("Test exception"));

        // Act and Assert
        await Assert.ThrowsExceptionAsync<RedisException>(async () => await _service.GetNotificationStatusAsync(key));
    }

    [TestMethod]
    public async Task InitializeUploadStatusAsync_Calls_Redis()
    {
        // Arrange
        var userAndOrganisationKeyPart = $"{_userAccount.User.Id}{_userAccount.User.Organisations[0].Id}";
        var key = $"{userAndOrganisationKeyPart}{SubsidiaryBulkUploadStatusKeys.SubsidiaryBulkUploadProgress}";
        var errorsKey = $"{userAndOrganisationKeyPart}{SubsidiaryBulkUploadStatusKeys.SubsidiaryBulkUploadErrors}";

        var expectedStatus = "Uploading";

        // Act
        await _service.InitializeUploadStatusAsync();

        // Assert
        _databaseMock.Verify(db => db.StringSetAsync(key, expectedStatus, null, false, When.Always, CommandFlags.None), Times.Once);
        _databaseMock.Verify(db => db.KeyDeleteAsync(errorsKey, It.IsAny<CommandFlags>()), Times.Once);

        _loggerMock.VerifyLog(x => x.LogInformation("Redis status: {Key} set to {Value} and any errors removed", key, expectedStatus), Times.Once);
    }

    [TestMethod]
    public async Task InitializeUploadStatusAsync_LogsAndRethrowsExceptionIfGetUserAccountFails()
    {
        _accountServiceClientMock.Setup(x => x.GetUserAccount(It.IsAny<Guid>())).ThrowsAsync(new HttpRequestException());

        await _service
            .Invoking(x => x.InitializeUploadStatusAsync())
        .Should()
        .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error getting user accounts with id {userId}", _userAccount.User.Id));
    }
}

using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StackExchange.Redis;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Models.Subsidiary;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class SubsidiaryServiceTests
{
    private Mock<ILogger<SubsidiaryService>> _loggerMock;
    private Mock<IConnectionMultiplexer> _connectionMultiplexerMock;
    private Mock<IDatabase> _databaseMock;

    private SubsidiaryService _service;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<SubsidiaryService>>();
        _databaseMock = new Mock<IDatabase>();
        _connectionMultiplexerMock = new Mock<IConnectionMultiplexer>();

        _connectionMultiplexerMock.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);

        _service = new SubsidiaryService(_loggerMock.Object, _connectionMultiplexerMock.Object);
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
        Assert.IsNotNull(result);
        Assert.IsNull(result.Errors);

        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Redis empty errors response key: {key}"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
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
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Errors.Count);
        Assert.AreEqual(1, result.Errors[0].FileLineNumber);
        Assert.AreEqual("Content1", result.Errors[0].FileContent);
        Assert.AreEqual("Message1", result.Errors[0].Message);
        Assert.AreEqual(6, result.Errors[0].ErrorNumber);
        Assert.IsTrue(result.Errors[0].IsError);
        Assert.AreEqual(2, result.Errors[1].FileLineNumber);
        Assert.AreEqual("Content2", result.Errors[1].FileContent);
        Assert.AreEqual("Message2", result.Errors[1].Message);
        Assert.IsFalse(result.Errors[1].IsError);
        Assert.AreEqual(9, result.Errors[1].ErrorNumber);
        Assert.AreEqual("Finished", result.Status);

        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Redis errors response key: {key} errors: {json}"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
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
        Assert.AreEqual(string.Empty, result);
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Redis empty status response key: {key}"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
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
        Assert.AreEqual(redisValue, result);
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Redis status response key: {key} value: {redisValue}"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
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
}

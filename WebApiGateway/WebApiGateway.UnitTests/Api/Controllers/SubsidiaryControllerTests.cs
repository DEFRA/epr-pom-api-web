using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Controllers;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Subsidiary;

namespace WebApiGateway.UnitTests.Api.Controllers;

[TestClass]
public class SubsidiaryControllerTests
{
    private Mock<ISubsidiaryService> _subsidiaryServiceMock;
    private Mock<ILogger<SubsidiaryController>> _loggerMock;
    private SubsidiaryController _controller;

    [TestInitialize]
    public void Setup()
    {
        _subsidiaryServiceMock = new Mock<ISubsidiaryService>();
        _loggerMock = new Mock<ILogger<SubsidiaryController>>();
        _controller = new SubsidiaryController(_subsidiaryServiceMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetNotificationErrors_ReturnsOk_WhenErrorsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var expectedErrors = new UploadFileErrorResponse
        {
            Errors = [new UploadFileErrorModel { FileLineNumber = 1, FileContent = "Content1", Message = "Message1", IsError = true }]
        };

        _subsidiaryServiceMock
            .Setup(s => s.GetNotificationErrorsAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedErrors);

        // Act
        var result = await _controller.GetNotificationErrors(userId, organisationId) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        result.Value.Should().Be(expectedErrors);
    }

    [TestMethod]
    public async Task GetNotificationErrors_ReturnsBadRequest_WhenExceptionThrown()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        _subsidiaryServiceMock
            .Setup(s => s.GetNotificationErrorsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetNotificationErrors(userId, organisationId) as BadRequestResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred during get notification errors from Redis.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [TestMethod]
    public async Task GetNotificationErrors_ReturnsOk_WhenNoErrorsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var expectedErrors = new UploadFileErrorResponse(); // Empty response

        _subsidiaryServiceMock
            .Setup(s => s.GetNotificationStatusAsync(It.IsAny<string>()))
            .ReturnsAsync("Test");

        expectedErrors.Status = "Test";

        _subsidiaryServiceMock
            .Setup(s => s.GetNotificationErrorsAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedErrors);

        // Act
        var result = await _controller.GetNotificationErrors(userId, organisationId) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        result.Value.Should().Be(expectedErrors);
    }
}

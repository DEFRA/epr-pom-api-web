using System.Net;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using WebApiGateway.Api.Clients;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Core.Models.RegistrationFeeCalculation;

namespace WebApiGateway.UnitTests.Api.Clients;

[TestClass]
public class RegistrationFeeCalculationDetailsClientTests
{
    private IFixture _fixture;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;
    private Mock<ILogger<IRegistrationFeeCalculationDetailsClient>> _loggerMock;
    private RegistrationFeeCalculationDetailsClient _client;

    [TestInitialize]
    public void SetUp()
    {
        _fixture = new Fixture();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _loggerMock = new Mock<ILogger<IRegistrationFeeCalculationDetailsClient>>();
        _client = new RegistrationFeeCalculationDetailsClient(_httpClient, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetFeeCalculationDetails_ShouldSendRequestWithCorrectUrl()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var largeProducerLateFeeDeadLine = new DateTime(2025, 10, 1);
        var smallProducerLateFeeDeadLine = new DateTime(2026, 4, 1);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(_fixture.Create<RegistrationFeeCalculationDetails[]>()))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == $"http://localhost/registration-fee-calculation-details/get-registration-fee-calculation-details/{fileId}/{largeProducerLateFeeDeadLine:o}/{smallProducerLateFeeDeadLine:o}"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage)
            .Verifiable("The URL in the request was not as expected.");

        // Act
        await _client.GetRegistrationFeeCalculationDetails(fileId, largeProducerLateFeeDeadLine, smallProducerLateFeeDeadLine);

        // Assert
        _httpMessageHandlerMock.Verify();
    }

    [TestMethod]
    public async Task GetFeeCalculationDetails_ShouldReturnFeeCalculationDetails_WhenResponseIsOk()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var largeProducerLateFeeDeadLine = new DateTime(2025, 10, 1);
        var smallProducerLateFeeDeadLine = new DateTime(2026, 4, 1);
        var expectedResponse = _fixture.Create<RegistrationFeeCalculationDetails[]>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(expectedResponse))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.GetRegistrationFeeCalculationDetails(fileId, largeProducerLateFeeDeadLine, smallProducerLateFeeDeadLine);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [TestMethod]
    public void GetFeeCalculationDetails_ShouldLogAndThrowHttpRequestException_WhenResponseIsBadRequest()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var largeProducerLateFeeDeadLine = new DateTime(2025, 10, 1);
        var smallProducerLateFeeDeadLine = new DateTime(2026, 4, 1);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            ReasonPhrase = "Bad Request"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _client.GetRegistrationFeeCalculationDetails(fileId, largeProducerLateFeeDeadLine, smallProducerLateFeeDeadLine);

        // Assert
        act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting registration fee calculation details, StatusCode : 400 (Bad Request)");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting registration fee calculation details, StatusCode : ")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public void GetFeeCalculationDetails_ShouldLogAndThrowHttpRequestException_WhenResponseIsNoContent()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var largeProducerLateFeeDeadLine = new DateTime(2025, 10, 1);
        var smallProducerLateFeeDeadLine = new DateTime(2026, 4, 1);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.NoContent)
        {
            ReasonPhrase = "No Content"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _client.GetRegistrationFeeCalculationDetails(fileId, largeProducerLateFeeDeadLine, smallProducerLateFeeDeadLine);

        // Assert
        act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting registration fee calculation details, StatusCode : 204 (No Content)");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting registration fee calculation details, StatusCode : ")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public void GetFeeCalculationDetails_ShouldLogAndThrowHttpRequestException_WhenResponseIsInternalServerError()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var largeProducerLateFeeDeadLine = new DateTime(2025, 10, 1);
        var smallProducerLateFeeDeadLine = new DateTime(2026, 4, 1);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            ReasonPhrase = "Internal Server Error"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _client.GetRegistrationFeeCalculationDetails(fileId, largeProducerLateFeeDeadLine, smallProducerLateFeeDeadLine);

        // Assert
        act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting registration fee calculation details, StatusCode : 500 (Internal Server Error)");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting registration fee calculation details, StatusCode : ")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}
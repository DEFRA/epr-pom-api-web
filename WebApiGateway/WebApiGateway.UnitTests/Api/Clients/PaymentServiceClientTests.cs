using System.Net;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using WebApiGateway.Api.Clients;
using WebApiGateway.Core.Models.RegistrationFeeCalculation;

namespace WebApiGateway.UnitTests.Api.Clients;

[TestClass]
public class PaymentServiceClientTests
{
    private IFixture _fixture;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;
    private Mock<ILogger<PaymentServiceClient>> _loggerMock;
    private PaymentServiceClient _client;

    [TestInitialize]
    public void SetUp()
    {
        _fixture = new Fixture();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _loggerMock = new Mock<ILogger<PaymentServiceClient>>();
        _client = new PaymentServiceClient(_httpClient, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetRegistrationFeeCalculationDetails_ShouldSendRequestWithCorrectUrl()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(_fixture.Create<RegistrationFeeCalculationDetails[]>()))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString() == $"http://localhost/v1/registration-submission-data/{submissionId}/fee-calculation-details"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage)
            .Verifiable("The URL in the request was not as expected.");

        // Act
        await _client.GetRegistrationFeeCalculationDetails(submissionId);

        // Assert
        _httpMessageHandlerMock.Verify();
    }

    [TestMethod]
    public async Task GetRegistrationFeeCalculationDetails_ShouldReturnDetails_WhenResponseIsOk()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var expectedDetails = _fixture.Create<RegistrationFeeCalculationDetails[]>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(expectedDetails))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.GetRegistrationFeeCalculationDetails(submissionId);

        // Assert
        result.Should().BeEquivalentTo(expectedDetails);
    }

    [TestMethod]
    public async Task GetRegistrationFeeCalculationDetails_ShouldReturnNull_WhenResponseIsNotFound()
    {
        // Arrange
        var submissionId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        var result = await _client.GetRegistrationFeeCalculationDetails(submissionId);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetRegistrationFeeCalculationDetails_ShouldLogInformation_WhenResponseIsNotFound()
    {
        // Arrange
        var submissionId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        await _client.GetRegistrationFeeCalculationDetails(submissionId);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registration fee calculation not found in payment service")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task GetRegistrationFeeCalculationDetails_ShouldReturnNullAndLogError_WhenResponseIsInternalServerError()
    {
        // Arrange
        var submissionId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act
        var result = await _client.GetRegistrationFeeCalculationDetails(submissionId);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred retrieving fee calculation details for submission id")),
                It.IsAny<HttpRequestException>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task GetRegistrationFeeCalculationDetails_ShouldReturnNullAndLogError_WhenResponseIsBadRequest()
    {
        // Arrange
        var submissionId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

        // Act
        var result = await _client.GetRegistrationFeeCalculationDetails(submissionId);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred retrieving fee calculation details for submission id")),
                It.IsAny<HttpRequestException>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }
}

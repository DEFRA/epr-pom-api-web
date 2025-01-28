using System.Net;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using WebApiGateway.Api.Clients;
using WebApiGateway.Core.Models.ProducerDetails;

namespace WebApiGateway.UnitTests.Api.Clients;

[TestClass]
public class ProducerDetailsClientTests
{
    private IFixture _fixture;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;
    private Mock<ILogger<ProducerDetailsClient>> _loggerMock;
    private ProducerDetailsClient _client;

    [TestInitialize]
    public void SetUp()
    {
        _fixture = new Fixture();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _loggerMock = new Mock<ILogger<ProducerDetailsClient>>();
        _client = new ProducerDetailsClient(_httpClient, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetProducerDetails_ShouldSendRequestWithCorrectUrl()
    {
        // Arrange
        var organisationId = _fixture.Create<int>();
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(_fixture.Create<GetProducerDetailsResponse>()))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == $"http://localhost/producer-details/get-producer-details/{organisationId}"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage)
            .Verifiable("The URL in the request was not as expected.");

        // Act
        await _client.GetProducerDetails(organisationId);

        // Assert
        _httpMessageHandlerMock.Verify();
    }

    [TestMethod]
    public async Task GetProducerDetails_ShouldReturnProducerDetails_WhenResponseIsOk()
    {
        // Arrange
        var expectedResponse = _fixture.Create<GetProducerDetailsResponse>();
        var organisationId = _fixture.Create<int>();
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
        var result = await _client.GetProducerDetails(organisationId);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [TestMethod]
    public void GetProducerDetails_ShouldLogAndThrowHttpRequestException_WhenResponseIsBadRequest()
    {
        // Arrange
        var organisationId = _fixture.Create<int>();
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
        Func<Task> act = async () => await _client.GetProducerDetails(organisationId);

        // Assert
        act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting Producer Details, Response status code does not indicate success: 400 (Bad Request)");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting Producer Details")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public void GetProducerDetails_ShouldLogAndThrowHttpRequestException_WhenResponseIsNoContent()
    {
        // Arrange
        var organisationId = _fixture.Create<int>();
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
        Func<Task> act = async () => await _client.GetProducerDetails(organisationId);

        // Assert
        act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting Producer Details, Response status code does not indicate success: 204 (No Content)");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting Producer Details")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public void GetProducerDetails_ShouldLogAndThrowHttpRequestException_WhenResponseIsInternalServerError()
    {
        // Arrange
        var organisationId = _fixture.Create<int>();
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
        Func<Task> act = async () => await _client.GetProducerDetails(organisationId);

        // Assert
        act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting Producer Details, Response status code does not indicate success: 500 (Internal Server Error)");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting Producer Details")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}
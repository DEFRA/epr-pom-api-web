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
using WebApiGateway.Core.Models.Antivirus;

[TestClass]
public class AntivirusClientTests
{
    private const string FileName = "filename.csv";
    private static readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private Mock<ILogger<AntivirusClient>> _loggerMock;
    private AntivirusClient _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _loggerMock = new Mock<ILogger<AntivirusClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://example.com")
        };

        _systemUnderTest = new AntivirusClient(httpClient, _loggerMock.Object);
    }

    [TestMethod]
    public async Task SendFileAsync_DoesNotThrowException_WhenHttpClientResponseIsCreated()
    {
        // Arrange
        var fileDetails = _fixture.Create<FileDetails>();
        var fileStream = new MemoryStream();

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.Created, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.SendFileAsync(fileDetails, FileName, fileStream))
            .Should()
            .NotThrowAsync();

        var expectedMethod = HttpMethod.Put;
        var expectedRequestUri = new Uri($"https://example.com/files/stream/{fileDetails.Collection}/{fileDetails.Key}");

        _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, Times.Once());
    }

    [TestMethod]
    public async Task SendFileAsync_ThrowException_WhenHttpClientResponseIsInternalServerError()
    {
        // Arrange
        var fileDetails = _fixture.Create<FileDetails>();
        var fileStream = new MemoryStream();

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.SendFileAsync(fileDetails, FileName, fileStream))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error sending file to antivirus api"));
    }
}
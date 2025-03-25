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
using WebApiGateway.Core.Models.Commondata;
using WebApiGateway.Core.Models.PackagingResubmissionApplication;

namespace WebApiGateway.UnitTests.Api.Clients;

[TestClass]
public class FeeCalculationDetailsClientTests
{
    private IFixture _fixture;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;
    private Mock<ILogger<ICommondataClient>> _loggerMock;
    private CommondataClient _client;

    [TestInitialize]
    public void SetUp()
    {
        _fixture = new Fixture();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _loggerMock = new Mock<ILogger<ICommondataClient>>();
        _client = new CommondataClient(_httpClient, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionMemberDetails_ShouldSendRequestWithCorrectUrl()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var complianceSchemeId = "test";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(_fixture.Create<PackagingResubmissionMemberResponse>()))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == $"http://localhost/submissions/pom-resubmission-paycal-parameters/{submissionId}?ComplianceSchemeId={complianceSchemeId}"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage)
            .Verifiable("The URL in the request was not as expected.");

        // Act
        await _client.GetPackagingResubmissionMemberDetails(submissionId, complianceSchemeId);

        // Assert
        _httpMessageHandlerMock.Verify();
    }

    [TestMethod]
    public async Task GetPackagingResubmissionMemberDetails_ShouldReturnPackagingResubmissionMember_WhenResponseIsOk()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var expectedResponse = _fixture.Create<PackagingResubmissionMemberResponse>();
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
        var result = await _client.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>());

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionMemberDetails_ShouldReturnPackagingResubmissionMember_WhenResponseIs428()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var expectedResponse = "Precondition failed error message.";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.PreconditionRequired)
        {
            Content = new StringContent(expectedResponse)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>());

        // Assert
        result.ErrorMessage.Should().BeEquivalentTo(expectedResponse);
    }

    [TestMethod]
    public void GetPackagingResubmissionMemberDetails_ShouldLogAndThrowHttpRequestException_WhenResponseIsBadRequest()
    {
        // Arrange
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
        Func<Task> act = async () => await _client.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>());

        // Assert
        act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting packaging resubmission member details for SubmissionId : {submissionId} and ComplianceSchemeId : {complianceSchemeId}", string.Empty, new Guid());

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting packaging resubmission member details ")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public void GetPackagingResubmissionMemberDetails_ShouldLogAndThrowHttpRequestException_WhenResponseIsNoContent()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(HttpStatusCode.PreconditionRequired)
        {
            ReasonPhrase = "No Reference number"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _client.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>());

        // Assert
        act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting packaging resubmission member details for SubmissionId : {submissionId} and ComplianceSchemeId : {complianceSchemeId}", string.Empty, new Guid());

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting packaging resubmission member details")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public void GetPackagingResubmissionMemberDetails_ShouldLogAndThrowHttpRequestException_WhenResponseIsInternalServerError()
    {
        // Arrange
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
        Func<Task> act = async () => await _client.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>());

        // Assert
        act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting packaging resubmission member details for SubmissionId : {submissionId} and ComplianceSchemeId : {complianceSchemeId}", string.Empty, new Guid());

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting packaging resubmission member details ")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionFileDetailsFromSynapse_ShouldSendRequestWithCorrectUrl()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var complianceSchemeId = "test";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(_fixture.Create<PackagingResubmissionMemberResponse>()))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == $"http://localhost/submissions/is_file_synced_with_cosmos/{fileId}"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage)
            .Verifiable("The URL in the request was not as expected.");

        // Act
        await _client.GetPackagingResubmissionFileDetailsFromSynapse(fileId);

        // Assert
        _httpMessageHandlerMock.Verify();
    }

    [TestMethod]
    public async Task GetPackagingResubmissionFileDetailsFromSynapse_ShouldReturnTrue_WhenResponseIsOk()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var expectedResponse = new SynapseResponse { IsFileSynced = true };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(true))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.GetPackagingResubmissionFileDetailsFromSynapse(It.IsAny<Guid>());

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [TestMethod]
    public void GetPackagingResubmissionFileDetailsFromSynapse_ShouldLogAndThrowHttpRequestException_WhenResponseIsBadRequest()
    {
        // Arrange
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
        Func<Task> act = async () => await _client.GetPackagingResubmissionFileDetailsFromSynapse(It.IsAny<Guid>());

        // Assert
        act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting file status from synapse, StatusCode : 400 (Bad Request)");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting file status from synapse, StatusCode : ")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public void GetPackagingResubmissionFileDetailsFromSynapse_ShouldLogAndThrowHttpRequestException_WhenResponseIsNoContent()
    {
        // Arrange
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
        Func<Task> act = async () => await _client.GetPackagingResubmissionFileDetailsFromSynapse(It.IsAny<Guid>());

        // Assert
        act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting file status from synapse, StatusCode : 204 (No Content)");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting file status from synapse, StatusCode : ")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [TestMethod]
    public void GetPackagingResubmissionFileDetailsFromSynapse_ShouldLogAndThrowHttpRequestException_WhenResponseIsInternalServerError()
    {
        // Arrange
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
        Func<Task> act = async () => await _client.GetPackagingResubmissionFileDetailsFromSynapse(It.IsAny<Guid>());

        // Assert
        act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting file status from synapse, StatusCode : 500 (Internal Server Error)");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting file status from synapse, StatusCode : ")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}
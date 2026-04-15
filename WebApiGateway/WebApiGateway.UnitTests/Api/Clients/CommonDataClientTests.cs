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
using WebApiGateway.Core.Models.PackagingResubmissionApplication;

namespace WebApiGateway.UnitTests.Api.Clients
{
    [TestClass]
    public class CommonDataClientTests
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
        public async Task GetPackagingResubmissionSyncStatusesFromSynapse_ShouldSendRequestsToBothEndpoints()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(_fixture.Create<bool>()))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == $"http://localhost/submissions/is_file_synced_with_cosmos/{fileId}"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable("The URL in the request was not as expected.");
            
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == $"http://localhost/submissions/is_pom_resubmission_synchronised/{fileId}"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable("The URL in the request was not as expected.");

            // Act
            await _client.GetPackagingResubmissionFileSyncStatusFromSynapse(fileId);
            await _client.GetPackagingResubmissionSyncStatusFromSynapse(fileId);

            // Assert
            _httpMessageHandlerMock.Verify();
        }

        [TestMethod]
        public async Task GetPackagingResubmissionFileSyncStatusFromSynapse_ShouldReturnTrue_WhenResponseIsOk()
        {
            // Arrange
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
            var result = await _client.GetPackagingResubmissionFileSyncStatusFromSynapse(It.IsAny<Guid>());

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void GetPackagingResubmissionFileSyncStatusFromSynapse_ShouldLogAndThrowHttpRequestException_WhenResponseIsBadRequest()
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
            Func<Task> act = async () => await _client.GetPackagingResubmissionFileSyncStatusFromSynapse(It.IsAny<Guid>());

            // Assert
            act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Error getting resubmission file sync status from Synapse, StatusCode : 400 (Bad Request)");

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error getting resubmission file sync status from Synapse, StatusCode : ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public void GetPackagingResubmissionFileSyncStatusFromSynapse_ShouldLogAndThrowHttpRequestException_WhenResponseIsNoContent()
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
            Func<Task> act = async () => await _client.GetPackagingResubmissionFileSyncStatusFromSynapse(It.IsAny<Guid>());

            // Assert
            act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Error getting resubmission file sync status from Synapse, StatusCode : 204 (No Content)");

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error getting resubmission file sync status from Synapse, StatusCode : ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public void GetPackagingResubmissionFileSyncStatusFromSynapse_ShouldLogAndThrowHttpRequestException_WhenResponseIsInternalServerError()
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
            Func<Task> act = async () => await _client.GetPackagingResubmissionFileSyncStatusFromSynapse(It.IsAny<Guid>());

            // Assert
            act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Error getting resubmission file sync status from Synapse, StatusCode : 500 (Internal Server Error)");

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error getting resubmission file sync status from Synapse, StatusCode : ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
        
        [TestMethod]
        public async Task GetPackagingResubmissionSyncStatusFromSynapse_ShouldReturnTrue_WhenResponseIsOk()
        {
            // Arrange
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
            var result = await _client.GetPackagingResubmissionSyncStatusFromSynapse(It.IsAny<Guid>());

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void GetPackagingResubmissionSyncStatusFromSynapse_ShouldLogAndThrowHttpRequestException_WhenResponseIsBadRequest()
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
            Func<Task> act = async () => await _client.GetPackagingResubmissionSyncStatusFromSynapse(It.IsAny<Guid>());

            // Assert
            act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Error getting resubmission sync status from Synapse, StatusCode : 400 (Bad Request)");

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error getting resubmission sync status from Synapse, StatusCode : ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public void GetPackagingResubmissionSyncStatusFromSynapse_ShouldLogAndThrowHttpRequestException_WhenResponseIsNoContent()
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
            Func<Task> act = async () => await _client.GetPackagingResubmissionSyncStatusFromSynapse(It.IsAny<Guid>());

            // Assert
            act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Error getting resubmission sync status from Synapse, StatusCode : 204 (No Content)");

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error getting resubmission sync status from Synapse, StatusCode : ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        [TestMethod]
        public void GetPackagingResubmissionSyncStatusFromSynapse_ShouldLogAndThrowHttpRequestException_WhenResponseIsInternalServerError()
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
            Func<Task> act = async () => await _client.GetPackagingResubmissionSyncStatusFromSynapse(It.IsAny<Guid>());

            // Assert
            act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Error getting resubmission sync status from Synapse, StatusCode : 500 (Internal Server Error)");

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error getting resubmission sync status from Synapse, StatusCode : ")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
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
            Guid.NewGuid();
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
            const string ExpectedResponse = "Precondition failed error message.";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.PreconditionRequired)
            {
                Content = new StringContent(ExpectedResponse)
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
            result?.ErrorMessage.Should().BeEquivalentTo(ExpectedResponse);
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
                .WithMessage("Error Getting packaging resubmission member details for SubmissionId : {submissionId} and ComplianceSchemeId : {complianceSchemeId}", string.Empty, Guid.NewGuid());

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
                .WithMessage("Error Getting packaging resubmission member details for SubmissionId : {submissionId} and ComplianceSchemeId : {complianceSchemeId}", string.Empty, Guid.NewGuid());

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
                .WithMessage("Error Getting packaging resubmission member details for SubmissionId : {submissionId} and ComplianceSchemeId : {complianceSchemeId}", string.Empty, Guid.NewGuid());

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
        public async Task GetActualSubmissionPeriod_ShouldReturnCorrectResponse_WhenSuccessful()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            const string SubmissionPeriod = "July to December 2025";
            const string ActualSubmissionPeriod = "January to December 2025";
            var responseMessageContent = _fixture.Create<PackagingResubmissionActualSubmissionPeriodResponse>();
            responseMessageContent.ActualSubmissionPeriod = ActualSubmissionPeriod;

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(responseMessageContent))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var response = await _client.GetActualSubmissionPeriod(submissionId, SubmissionPeriod);

            // Assert
            _httpMessageHandlerMock.Verify();
            response.ActualSubmissionPeriod.Should().Be(ActualSubmissionPeriod);
        }

        [TestMethod]
        public void GetActualSubmissionPeriod_ShouldLogAndThrowHttpRequestException_WhenResponseIsBadRequest()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var submissionPeriod = "July to December 2025";
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
            Func<Task> act = async () => await _client.GetActualSubmissionPeriod(It.IsAny<Guid>(), It.IsAny<string>());

            // Assert
            act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Error Getting actual submission period for submission, StatusCode : {StatusCode} ({ReasonPhrase}), SubmissionId: {SubmissionId}, Submission Period: {submissionPeriod}", "400", string.Empty, submissionId, submissionPeriod);

            _loggerMock.Verify(
            x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Getting actual submission period for submission")),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
        }
    }
}

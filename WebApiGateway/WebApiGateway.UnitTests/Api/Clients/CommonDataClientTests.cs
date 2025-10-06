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
        public async Task GetActualSubmissionPeriod_ShouldReturnCorrectResponse_WhenSuccessful()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var submissionPeriod = "July to December 2025";
            var actualSubmissionPeriod = "January to December 2025";
            var complianceSchemeId = "test";
            var responseMessageContent = _fixture.Create<PackagingResubmissionActualSubmissionPeriodResponse>();
            responseMessageContent.ActualSubmissionPeriod = actualSubmissionPeriod;

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
            var response = await _client.GetActualSubmissionPeriod(submissionId, submissionPeriod);

            // Assert
            _httpMessageHandlerMock.Verify();
            response.ActualSubmissionPeriod.Should().Be(actualSubmissionPeriod);
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

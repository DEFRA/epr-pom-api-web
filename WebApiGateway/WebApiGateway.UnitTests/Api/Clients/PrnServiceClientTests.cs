using System.Net;
using AutoFixture.MSTest;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Core.Models.Prns;
using WebApiGateway.UnitTests.Support.Extensions;

namespace WebApiGateway.UnitTests.Api.Clients
{
    [TestClass]
    public class PrnServiceClientTests
    {
        private Mock<ILogger<PrnServiceClient>> _loggerMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private IPrnServiceClient _systemUnderTest;

        [TestInitialize]
        public void TestInitialize()
        {
            _loggerMock = new Mock<ILogger<PrnServiceClient>>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://example.com")
            };

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _systemUnderTest = new PrnServiceClient(httpClient, _loggerMock.Object);
        }

        [TestMethod]
        [AutoData]
        public async Task GetAllPrnsForOrganisation_ReturnListOfPrns(Guid orgId, List<PrnModel> response)
        {
            _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, response.ToJsonContent());

            var result = await _systemUnderTest.GetAllPrnsForOrganisation(orgId);

            var expectedMethod = HttpMethod.Get;
            var expectedRequestUri = new Uri($"https://example.com/prn/organisation?orgId={orgId}");

            _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, Times.Once());
            result.Should().BeOfType<List<PrnModel>>();
            result.Should().BeEquivalentTo(response);
        }

        [TestMethod]
        [AutoData]
        public async Task GetAllPrnsForOrganisation_ThrowsExceptionIfStatusIsNotSUccess(Guid orgId)
        {
            _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

            await _systemUnderTest
                .Invoking(x => x.GetAllPrnsForOrganisation(orgId))
                .Should()
            .ThrowAsync<HttpRequestException>();

            _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "An error occurred retrieving prns for organisation {organisationId}", orgId));
        }

        [TestMethod]
        [AutoData]
        public async Task GetPrnById_ReturnPrns(Guid prnId, PrnModel response)
        {
            _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, response.ToJsonContent());

            var result = await _systemUnderTest.GetPrnById(prnId);

            var expectedMethod = HttpMethod.Get;
            var expectedRequestUri = new Uri($"https://example.com/prn/{prnId}");

            _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, Times.Once());
            result.Should().BeOfType<PrnModel>();
            result.Should().BeEquivalentTo(response);
        }

        [TestMethod]
        [AutoData]
        public async Task GetPrnById_ThrowsExceptionIfStatusIsNotSUccess(Guid prnId)
        {
            _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

            await _systemUnderTest
                .Invoking(x => x.GetPrnById(prnId))
                .Should()
            .ThrowAsync<HttpRequestException>();

            _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "An error occurred retrieving prns for Id {prnId}", prnId));
        }

        [TestMethod]
        [AutoData]
        public async Task UpdatePrnStatusToAccepted_ReturnNoContent(Guid prnId)
        {
            _httpMessageHandlerMock.RespondWith(HttpStatusCode.NoContent, null);

            await _systemUnderTest.UpdatePrnStatusToAccepted(prnId);

            var expectedMethod = HttpMethod.Patch;
            var expectedRequestUri = new Uri($"https://example.com/prn/status/{prnId}");

            _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, Times.Once());
        }

        [TestMethod]
        [AutoData]
        public async Task UpdatePrnStatusToAccepted_ThrowsExceptionIfStatusIsNotSUccess(Guid prnId)
        {
            _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

            await _systemUnderTest
                .Invoking(x => x.UpdatePrnStatusToAccepted(prnId))
                .Should()
            .ThrowAsync<HttpRequestException>();

            _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "An error occurred updating prns status for Id {prnId}", prnId));
        }
    }
}

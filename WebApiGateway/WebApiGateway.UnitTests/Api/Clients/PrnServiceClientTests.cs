using System.Net;
using System.Security.Claims;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using WebApiGateway.Api.Clients;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Pagination;
using WebApiGateway.Core.Models.Prns;
using WebApiGateway.Core.Models.UserAccount;
using WebApiGateway.UnitTests.Support.Extensions;

namespace WebApiGateway.UnitTests.Api.Clients
{
    [TestClass]
    public class PrnServiceClientTests
    {
        private static readonly IFixture _fixture = new Fixture();
        private readonly UserAccount _userAccount = _fixture.Create<UserAccount>();
        private Mock<ILogger<PrnServiceClient>> _loggerMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private HttpClient _httpClient;
        private Mock<IAccountServiceClient> _accountServiceClientMock;
        private Mock<IConfiguration> _configurationMock;
        private PrnServiceClient _systemUnderTest;
        private Mock<IComplianceSchemeDetailsService> _complianceSchemeSvcMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _loggerMock = new Mock<ILogger<PrnServiceClient>>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _complianceSchemeSvcMock = new Mock<IComplianceSchemeDetailsService>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://example.com")
            };

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();

            claimsPrincipalMock.Setup(x => x.Claims).Returns(new List<Claim>
            {
                new(ClaimConstants.ObjectId, _userAccount.User.Id.ToString())
            });

            _accountServiceClientMock = new Mock<IAccountServiceClient>();
            _accountServiceClientMock.Setup(x => x.GetUserAccount(_userAccount.User.Id)).ReturnsAsync(_userAccount);
            httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipalMock.Object);

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["LogPrefix"]).Returns("[WebApiGateway]");

            _systemUnderTest = new PrnServiceClient(_httpClient, _loggerMock.Object, httpContextAccessorMock.Object, _accountServiceClientMock.Object, _configurationMock.Object, _complianceSchemeSvcMock.Object);
        }

        [TestMethod]
        public async Task GetAllPrnsForOrganisation_ReturnListOfPrns()
        {
            var response = _fixture.CreateMany<PrnModel>().ToList();
            _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, response.ToJsonContent());

            var result = await _systemUnderTest.GetAllPrnsForOrganisation();

            var expectedMethod = HttpMethod.Get;
            var expectedRequestUri = new Uri($"https://example.com/v1/prn/organisation");

            _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, Times.Once());
            result.Should().BeOfType<List<PrnModel>>();
            result.Should().BeEquivalentTo(response);
        }

        [TestMethod]
        public async Task GetAllPrnsForOrganisation_ThrowsExceptionIfStatusIsNotSUccess()
        {
            _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);
            await _systemUnderTest.Invoking(x => x.GetAllPrnsForOrganisation()).Should().ThrowAsync<HttpRequestException>();
        }

        [TestMethod]
        public async Task GetPrnById_ReturnPrns()
        {
            var prnId = Guid.NewGuid();
            var response = _fixture.Create<PrnModel>();

            _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, response.ToJsonContent());

            var result = await _systemUnderTest.GetPrnById(prnId);

            var expectedMethod = HttpMethod.Get;
            var expectedRequestUri = new Uri($"https://example.com/v1/prn/{prnId}");

            _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, Times.Once());
            result.Should().BeOfType<PrnModel>();
            result.Should().BeEquivalentTo(response);
        }

        [TestMethod]
        public async Task GetPrnById_ThrowsExceptionIfStatusIsNotSUccess()
        {
            var prnId = Guid.NewGuid();
            _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

            await _systemUnderTest.Invoking(x => x.GetPrnById(prnId)).Should().ThrowAsync<HttpRequestException>();
        }

        [TestMethod]
        public async Task UpdatePrnStatusToAccepted_ReturnNoContent()
        {
            var updatePrns = _fixture.CreateMany<UpdatePrnStatus>().ToList();
            _httpMessageHandlerMock.RespondWith(HttpStatusCode.NoContent, null);

            await _systemUnderTest.UpdatePrnStatus(updatePrns);

            var expectedMethod = HttpMethod.Post;
            var expectedRequestUri = new Uri($"https://example.com/v1/prn/status");

            _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, Times.Once());
        }

        [TestMethod]
        public async Task UpdatePrnStatusToAccepted_ThrowsExceptionIfStatusIsNotSUccess()
        {
            var updatePrns = _fixture.CreateMany<UpdatePrnStatus>().ToList();
            _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

            await _systemUnderTest.Invoking(x => x.UpdatePrnStatus(updatePrns)).Should().ThrowAsync<HttpRequestException>();
        }

        [TestMethod]
        public async Task GetObligationCalculationByYear_ReturnsCalculations()
        {
            int year = DateTime.Now.Year;
            var calculations = _fixture.Create<ObligationModel>();
            _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, calculations.ToJsonContent());

            var result = await _systemUnderTest.GetObligationCalculationByYearAsync(year);

            var expectedMethod = HttpMethod.Get;
            var expectedRequestUri = new Uri($"https://example.com/v1/prn/obligationcalculation/{year}");

            _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, Times.Once());
            result.Should().BeOfType<ObligationModel>();
            result.Should().BeEquivalentTo(calculations);
        }

        [TestMethod]
        public async Task ConfigureHttpClientAsync_LogsAndRethrowsExceptionIfGetUserAccountFails()
        {
            _accountServiceClientMock.Setup(x => x.GetUserAccount(It.IsAny<Guid>())).ThrowsAsync(new HttpRequestException());

            await _systemUnderTest.Invoking(x => x.GetAllPrnsForOrganisation()).Should().ThrowAsync<HttpRequestException>();
        }

        [TestMethod]
        public async Task GetSearchPrns_ShouldReturnPaginatedResponse_WhenRequestIsValid()
        {
            // Arrange
            var request = new PaginatedRequest();
            var expectedResponse = new PaginatedResponse<PrnModel>
            {
                Items = new List<PrnModel> { new() { Id = 1, PrnNumber = "PRN123" } },
                CurrentPage = 1,
                TotalItems = 1,
                PageSize = 10
            };
            var jsonResponse = JsonConvert.SerializeObject(expectedResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            };
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(httpResponseMessage));

            // Act
            var result = await _systemUnderTest.GetSearchPrns(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task GetSearchPrns_ShouldReturnEmptyResponse_WhenNoPrnsFound()
        {
            // Arrange
            var request = new PaginatedRequest();
            var expectedResponse = new PaginatedResponse<PrnModel>
            {
                Items = new List<PrnModel>(),
                CurrentPage = 1,
                TotalItems = 0,
                PageSize = 10
            };
            var jsonResponse = JsonConvert.SerializeObject(expectedResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            };
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(httpResponseMessage));

            // Act
            var result = await _systemUnderTest.GetSearchPrns(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task GetSearchPrns_ShouldReturnPaginatedResponse_WhenMultiplePrnsFound()
        {
            // Arrange
            var request = new PaginatedRequest();
            var expectedResponse = new PaginatedResponse<PrnModel>
            {
                Items = new List<PrnModel>
                {
                    new() { Id = 1, PrnNumber = "PRN123" },
                    new() { Id = 2, PrnNumber = "PRN456" }
                },
                CurrentPage = 1,
                TotalItems = 2,
                PageSize = 10
            };
            var jsonResponse = JsonConvert.SerializeObject(expectedResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            };
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(httpResponseMessage));

            // Act
            var result = await _systemUnderTest.GetSearchPrns(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task GetSearchPrns_ShouldLogErrorAndThrowException_WhenHttpRequestFails()
        {
            // Arrange
            var request = _fixture.Create<PaginatedRequest>();
            var exception = new HttpRequestException("Request failed");
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(exception);

            // Act
            Func<Task> act = async () => await _systemUnderTest.GetSearchPrns(request);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>().WithMessage("Request failed");
        }

        [TestMethod]
        public async Task GetObligationCalculationByYearAsync_LogsErrorAndThrowsException_OnHttpRequestException()
        {
            // Arrange
            int year = DateTime.Now.Year;
            var exceptionMessage = "Test HttpRequestException";
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException(exceptionMessage));
            var logEntries = new List<string>();
            _loggerMock
                .Setup(logger => logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback<LogLevel, EventId, object, Exception, Delegate>((logLevel, eventId, state, exception, formatter) =>
                {
                    logEntries.Add(state.ToString());
                });

            // Act
            Func<Task> act = async () => await _systemUnderTest.GetObligationCalculationByYearAsync(year);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>().WithMessage(exceptionMessage);
            logEntries.Should().Contain(log =>
                log.Contains("PrnServiceClient - GetObligationCalculationByYearAsync: An error occurred retrievig obligation calculations for organisation") &&
                log.Contains("v1/prn/obligationcalculation") &&
                log.Contains(year.ToString()));
        }
    }
}

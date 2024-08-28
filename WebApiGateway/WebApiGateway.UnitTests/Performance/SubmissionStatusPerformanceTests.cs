using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Core.Models.ProducerValidation;
using WebApiGateway.Core.Models.RegistrationValidation;
using WebApiGateway.Core.Models.UserAccount;
using WebApiGateway.UnitTests.Support.Extensions;

namespace WebApiGateway.UnitTests.Performance;

[TestClass]
public class SubmissionStatusPerformanceTests
{
    private static readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly UserAccount _userAccount = _fixture.Create<UserAccount>();

    private Mock<ILogger<SubmissionStatusClient>> _loggerMock;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private SubmissionStatusClient _systemUnderTest;

    public SubmissionStatusPerformanceTests()
    {
        _loggerMock = new Mock<ILogger<SubmissionStatusClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://example.com")
        };

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();

        claimsPrincipalMock.Setup(x => x.Claims).Returns(new List<Claim>
        {
            new(ClaimConstants.ObjectId, _userAccount.User.Id.ToString())
        });

        var accountServiceClientMock = new Mock<IAccountServiceClient>();
        accountServiceClientMock.Setup(x => x.GetUserAccount(_userAccount.User.Id)).ReturnsAsync(_userAccount);
        httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipalMock.Object);
        _systemUnderTest = new SubmissionStatusClient(httpClient, accountServiceClientMock.Object, httpContextAccessorMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetProducerValidationWarningRowsAsync_WhenCalledWith1000Warnings_ReturnsWarningsBackInLessThan100ms()
    {
        // Arrange
        var stopwatch = new Stopwatch();
        var submissionId = Guid.NewGuid();
        var validationWarningRows = _fixture.Build<ProducerValidationIssueRow>()
            .With(x => x.Issue, "Warning")
            .CreateMany(1000)
            .ToList();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, validationWarningRows.ToJsonContent());

        // Act
        stopwatch.Start();
        await _systemUnderTest.GetProducerValidationWarningRowsAsync(submissionId);
        stopwatch.Stop();

        // Assert
        var elapsedTime = stopwatch.ElapsedMilliseconds;

        Console.WriteLine($"Fetching {validationWarningRows.Count} validation warning rows took {elapsedTime} milliseconds");
        elapsedTime.Should().BeLessThan(100, $"Expected elapsed time should take 100ms but instead took {elapsedTime}ms");
    }

    [TestMethod]
    public async Task GetProducerValidationErrorRowsAsync_WhenCalledWith1000Warnings_ReturnsWarningsBackInLessThan100ms()
    {
        // Arrange
        var stopwatch = new Stopwatch();
        var submissionId = Guid.NewGuid();
        var validationErrorRows = _fixture.Build<ProducerValidationIssueRow>()
            .With(x => x.Issue, "Error")
            .CreateMany(1000)
            .ToList();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, validationErrorRows.ToJsonContent());

        // Act
        stopwatch.Start();
        await _systemUnderTest.GetProducerValidationErrorRowsAsync(submissionId);
        stopwatch.Stop();

        // Assert
        var elapsedTime = stopwatch.ElapsedMilliseconds;

        Console.WriteLine($"Fetching {validationErrorRows.Count} validation error rows took {elapsedTime} milliseconds");
        elapsedTime.Should().BeLessThan(100, $"Expected elapsed time should take 100ms but instead took {elapsedTime}ms");
    }

    [TestMethod]
    public async Task GetRegistrationValidationErrorAsync_WhenCalledWith1000Errors_ReturnsErrorsBackInLessThan100ms()
    {
        // Arrange
        var stopwatch = new Stopwatch();
        var submissionId = Guid.NewGuid();
        var registrationValidationErrorRows = _fixture.Build<RegistrationValidationError>()
            .CreateMany(1000)
            .ToList();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, registrationValidationErrorRows.ToJsonContent());

        // Act
        stopwatch.Start();
        await _systemUnderTest.GetRegistrationValidationErrorsAsync(submissionId);
        stopwatch.Stop();

        // Assert
        var elapsedTime = stopwatch.ElapsedMilliseconds;

        Console.WriteLine($"Fetching {registrationValidationErrorRows.Count} validation error rows took {elapsedTime} milliseconds");
        elapsedTime.Should().BeLessThan(100, $"Expected elapsed time should take 100ms but instead took {elapsedTime}ms");
    }
}
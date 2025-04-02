using System.Diagnostics;
using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Common.Logging.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Models.ProducerValidation;
using WebApiGateway.Core.Models.RegistrationValidation;
using WebApiGateway.Core.Options;

namespace WebApiGateway.UnitTests.Performance;

[TestClass]
public class SubmissionServicePerformanceTests
{
    private const string RegistrationContainerName = "registration-container-name";
    private const string PomContainerName = "pom-container-name";

    private static readonly IFixture Fixture = new Fixture().Customize(new AutoMoqCustomization());

    private Mock<ISubmissionStatusClient> _submissionStatusClientMock;
    private Mock<ILoggingService> _loggingServiceMock;
    private Mock<ILogger<SubmissionService>> _loggerMock;

    private SubmissionService _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _submissionStatusClientMock = new Mock<ISubmissionStatusClient>();
        var options = Options.Create(
            new StorageAccountOptions
            {
                RegistrationContainer = RegistrationContainerName,
                PomContainer = PomContainerName
            });
        _loggingServiceMock = new Mock<ILoggingService>();
        _loggerMock = new Mock<ILogger<SubmissionService>>();
        _systemUnderTest = new SubmissionService(_submissionStatusClientMock.Object, _loggingServiceMock.Object, _loggerMock.Object, options);
    }

    [TestMethod]
    public async Task GetProducerValidationIssuesAsync_WhenWorkingWith2000Issues_BenchmarksElapsedTimeTo200Milliseconds()
    {
        // Arrange
        var stopwatch = new Stopwatch();
        var submissionId = Guid.NewGuid();

        var validationErrorRows = GenerateRandomProducerValidationIssueList().ToList();
        var validationWarningRows = GenerateRandomProducerValidationIssueList().ToList();

        _submissionStatusClientMock.Setup(x => x.GetProducerValidationErrorRowsAsync(submissionId)).ReturnsAsync(validationErrorRows);
        _submissionStatusClientMock.Setup(x => x.GetProducerValidationWarningRowsAsync(submissionId)).ReturnsAsync(validationWarningRows);

        // Act
        stopwatch.Start();
        await _systemUnderTest.GetProducerValidationIssuesAsync(submissionId);
        stopwatch.Stop();

        // Assert
        var elapsedTime = stopwatch.ElapsedMilliseconds;

        Console.WriteLine($"Fetching {validationWarningRows.Count + validationErrorRows.Count} validation issues rows took {elapsedTime} milliseconds");
        elapsedTime.Should().BeLessThan(200, $"Expected elapsed time should take 200ms but instead took {elapsedTime}ms");
    }

    [TestMethod]
    public async Task GetRegistrationValidationErrorsAsync_WhenWorkingWith2000Issues_BenchmarksElapsedTimeTo200Milliseconds()
    {
        // Arrange
        var stopwatch = new Stopwatch();
        var submissionId = Guid.NewGuid();

        var registrationValidationErrorRows = GenerateRandomRegistrationValidationIssueList(false).ToList();
        var registrationValidationWarningRows = GenerateRandomRegistrationValidationIssueList(true).ToList();

        _submissionStatusClientMock.Setup(x => x.GetRegistrationValidationErrorsAsync(submissionId)).ReturnsAsync(registrationValidationErrorRows);
        _submissionStatusClientMock.Setup(x => x.GetRegistrationValidationWarningsAsync(submissionId)).ReturnsAsync(registrationValidationWarningRows);

        // Act
        stopwatch.Start();
        await _systemUnderTest.GetRegistrationValidationErrorsAsync(submissionId);
        stopwatch.Stop();

        // Assert
        var elapsedTime = stopwatch.ElapsedMilliseconds;

        Console.WriteLine($"Fetching {registrationValidationErrorRows.Count + registrationValidationErrorRows.Count} validation issues rows took {elapsedTime} milliseconds");
        elapsedTime.Should().BeLessThan(200, $"Expected elapsed time should take 200ms but instead took {elapsedTime}ms");
    }

    private static List<ProducerValidationIssueRow> GenerateRandomProducerValidationIssueList()
    {
        var random = new Random();
        return Fixture
            .Build<ProducerValidationIssueRow>()
            .With(x => x.RowNumber, random.Next(1, 20))
            .CreateMany(1000)
            .ToList();
    }

    private static List<RegistrationValidationError> GenerateRandomRegistrationValidationIssueList(bool isWarning)
    {
        var fixture = Fixture
               .Build<RegistrationValidationError>()
               .CreateMany(1000)
               .ToList();

        if (isWarning)
        {
            fixture.ForEach(x => x.IssueType = IssueType.Warning);
        }
        else
        {
            fixture.ForEach(x => x.IssueType = IssueType.Error);
        }

        Fixture.Inject(fixture);
        return fixture;
    }
}
using System.Net;
using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Common.Logging.Constants;
using EPR.Common.Logging.Models;
using EPR.Common.Logging.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.ProducerValidation;
using WebApiGateway.Core.Models.Submission;
using WebApiGateway.Core.Options;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class SubmissionServiceTests
{
    private const string _registrationContainerName = "registration-container-name";
    private const string _pomContainerName = "pom-container-name";

    private static readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

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
                RegistrationContainer = _registrationContainerName,
                PomContainer = _pomContainerName
            });
        _loggingServiceMock = new Mock<ILoggingService>();
        _loggerMock = new Mock<ILogger<SubmissionService>>();
        _systemUnderTest = new SubmissionService(_submissionStatusClientMock.Object, _loggingServiceMock.Object, _loggerMock.Object, options);
    }

    [TestMethod]
    public async Task CreateSubmissionAsync_ReturnsSubmissionId_WhenSubmissionIsCreatedSuccessfully()
    {
        // Arrange
        const SubmissionType SubmissionType = SubmissionType.Producer;
        const string SubmissionPeriod = "Jan to Jun 23";

        // Act
        await _systemUnderTest.CreateSubmissionAsync(SubmissionType, SubmissionPeriod, null);

        // Assert
        _submissionStatusClientMock.Verify(
            x => x.CreateSubmissionAsync(
                It.Is<CreateSubmission>(m =>
                    m.SubmissionType == SubmissionType
                    && m.SubmissionPeriod == SubmissionPeriod
                    && m.DataSourceType == DataSourceType.File
                    && m.Id != Guid.Empty)),
            Times.Once);
    }

    [TestMethod]
    public async Task CreateAntivirusCheckEventAsync_ReturnsFileId_WhenAntivirusCheckEventAndMonitoringEventAreCreatedSuccessfully()
    {
        // Arrange
        const string Filename = "filename.csv";
        const FileType FileType = FileType.Pom;
        var submissionId = Guid.NewGuid();
        var registrationSetId = Guid.NewGuid();

        // Act
        var result = await _systemUnderTest.CreateAntivirusCheckEventAsync(Filename, FileType, submissionId, registrationSetId);

        // Assert
        result.As<Guid>().Should().NotBeEmpty();

        _submissionStatusClientMock.Verify(
            x => x.CreateEventAsync(
                It.Is<AntivirusCheckEvent>(m =>
                    m.FileName == Filename
                    && m.FileType == FileType
                    && m.BlobContainerName == _pomContainerName
                    && m.FileId != Guid.Empty
                    && m.RegistrationSetId == registrationSetId),
                submissionId),
            Times.Once);

        _loggingServiceMock.Verify(
            x => x.SendEventAsync(
                It.Is<ProtectiveMonitoringEvent>(m =>
                    m.SessionId == submissionId
                    && m.Component == "epr_pom_api_web"
                    && m.PmcCode == PmcCodes.Code0212
                    && m.Priority == 0
                    && m.TransactionCode == TransactionCodes.Uploaded
                    && m.Message == Filename)),
            Times.Once);
    }

    [TestMethod]
    public async Task CreateAntivirusCheckEventAsync_ReturnsFileId_WhenAntivirusCheckEventCreatedSuccessfullyButMonitoringEventIsUnsuccessful()
    {
        // Arrange
        const string Filename = "filename.csv";
        const FileType FileType = FileType.Pom;
        var submissionId = Guid.NewGuid();
        var httpRequestException = new HttpRequestException();

        _loggingServiceMock.Setup(x => x.SendEventAsync(It.IsAny<ProtectiveMonitoringEvent>())).ThrowsAsync(httpRequestException);

        // Act
        var result = await _systemUnderTest.CreateAntivirusCheckEventAsync(Filename, FileType, submissionId, null);

        // Assert
        result.As<Guid>().Should().NotBeEmpty();

        _submissionStatusClientMock.Verify(
            x => x.CreateEventAsync(
                It.Is<AntivirusCheckEvent>(m =>
                    m.FileName == Filename
                    && m.FileType == FileType
                    && m.BlobContainerName == _pomContainerName
                    && m.FileId != Guid.Empty),
                submissionId),
            Times.Once);

        _loggingServiceMock.Verify(x => x.SendEventAsync(It.IsAny<ProtectiveMonitoringEvent>()), Times.Once);
        _loggerMock.VerifyLog(x => x.LogError(httpRequestException, "An error occurred creating the protective monitoring event"));
    }

    [TestMethod]
    public async Task GetSubmissionAsync_ReturnsHttpResponseMessage()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var responseBody = "response body";
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseBody)
        };
        _submissionStatusClientMock.Setup(x => x.GetSubmissionAsync(submissionId)).ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _systemUnderTest.GetSubmissionAsync(submissionId);

        // Assert
        result.StatusCode.Should().Be(httpResponseMessage.StatusCode);
        (await result.Content.ReadAsStringAsync()).Should().Be(responseBody);

        _submissionStatusClientMock.Verify(x => x.GetSubmissionAsync(submissionId), Times.Once);
    }

    [TestMethod]
    public async Task GetSubmissionsAsync_ReturnsSubmissions()
    {
        // Arrange
        const string QueryString = "?key=value";
        var submissions = _fixture.Create<List<AbstractSubmission>>();
        _submissionStatusClientMock.Setup(x => x.GetSubmissionsAsync(QueryString)).ReturnsAsync(submissions);

        // Act
        var result = await _systemUnderTest.GetSubmissionsAsync(QueryString);

        // Assert
        result.Should().BeEquivalentTo(submissions);

        _submissionStatusClientMock.Verify(x => x.GetSubmissionsAsync(QueryString), Times.Once);
    }

    [TestMethod]
    public async Task GetProducerValidationIssuesAsync_ReturnsProducerValidationIssueRowsGroupedByRowNumber()
    {
        // Arrange
        var submissionId = Guid.NewGuid();

        var validationErrorRows = GenerateRandomIssueList().ToList();
        var validationWarningRows = GenerateRandomIssueList().ToList();

        _submissionStatusClientMock.Setup(x => x.GetProducerValidationErrorRowsAsync(submissionId)).ReturnsAsync(validationErrorRows);
        _submissionStatusClientMock.Setup(x => x.GetProducerValidationWarningRowsAsync(submissionId)).ReturnsAsync(validationWarningRows);

        // Act
        var result = await _systemUnderTest.GetProducerValidationIssuesAsync(submissionId);

        // Assert
        result.Should().BeEquivalentTo(validationErrorRows.Concat(validationWarningRows));
        result.Should().BeInAscendingOrder(x => x.RowNumber);

        _submissionStatusClientMock.Verify(x => x.GetProducerValidationErrorRowsAsync(submissionId), Times.Once);
    }

    [TestMethod]
    public async Task SubmitAsync_CallsSubmissionStatusClient()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var submissionPayload = new SubmissionPayload
        {
            SubmittedBy = "Test Name",
            FileId = Guid.NewGuid()
        };

        // Act
        await _systemUnderTest.SubmitAsync(submissionId, submissionPayload);

        // Assert
        _submissionStatusClientMock.Verify(x => x.SubmitAsync(submissionId, submissionPayload), Times.Once);
    }

    private static IEnumerable<ProducerValidationIssueRow> GenerateRandomIssueList()
    {
        var random = new Random();
        return _fixture
            .Build<ProducerValidationIssueRow>()
            .With(x => x.RowNumber, random.Next(1, 20))
            .CreateMany(10)
            .ToList();
    }
}
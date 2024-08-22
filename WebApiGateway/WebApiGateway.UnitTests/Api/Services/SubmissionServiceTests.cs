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
using WebApiGateway.Core.Models.RegistrationValidation;
using WebApiGateway.Core.Models.Submission;
using WebApiGateway.Core.Models.SubmissionHistory;
using WebApiGateway.Core.Models.Submissions;
using WebApiGateway.Core.Options;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class SubmissionServiceTests
{
    private const string _registrationContainerName = "registration-container-name";
    private const string _pomContainerName = "pom-container-name";
    private const string _subsidiaryContainerName = "subsidiary-container-name";

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
                PomContainer = _pomContainerName,
                SubsidiaryContainer = _subsidiaryContainerName,
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
    [DataRow(FileType.Pom, _pomContainerName)]
    [DataRow(FileType.Brands, _registrationContainerName)]
    [DataRow(FileType.Subsidiaries, _subsidiaryContainerName)]
    public async Task CreateAntivirusCheckEventAsync_ReturnsFileId_WhenAntivirusCheckEventAndMonitoringEventAreCreatedSuccessfully(FileType fileType, string containerName)
    {
        // Arrange
        const string Filename = "filename.csv";
        var submissionId = Guid.NewGuid();
        var registrationSetId = Guid.NewGuid();

        // Act
        var result = await _systemUnderTest.CreateAntivirusCheckEventAsync(Filename, fileType, submissionId, registrationSetId);

        // Assert
        result.As<Guid>().Should().NotBeEmpty();

        _submissionStatusClientMock.Verify(
            x => x.CreateEventAsync(
                It.Is<AntivirusCheckEvent>(m =>
                    m.FileName == Filename
                    && m.FileType == fileType
                    && m.BlobContainerName == containerName
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

        var validationErrorRows = GenerateRandomProducerValidationIssueList().ToList();
        var validationWarningRows = GenerateRandomProducerValidationIssueList().ToList();

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

    [TestMethod]
    public async Task GetSubmissionPeriodHistory_WhenCalledWithoutDecisions_WillLeaveStatusEmpty()
    {
        // Arrange
        const string queryString = "?key=value";
        var submissionId = Guid.NewGuid();
        var submissionHistoryEventsResponse = new SubmissionHistoryEventsResponse
        {
            SubmittedEvents = _fixture.Build<SubmittedEventResponse>()
            .CreateMany(2)
            .ToList(),
            RegulatorDecisionEvents = new List<RegulatorDecisionEventResponse>(),
            AntivirusCheckEvents = new List<AntivirusCheckEventResponse>()
        };

        _submissionStatusClientMock.Setup(x => x.GetSubmissionPeriodHistory(submissionId, queryString)).ReturnsAsync(submissionHistoryEventsResponse);

        // Act
        var result = await _systemUnderTest.GetSubmissionPeriodHistory(submissionId, queryString);

        // Assert
        result.Should().NotBeNull().And.HaveCount(2);

        for (int i = 0; i < result.Count; i++)
        {
            var submittedEvent = submissionHistoryEventsResponse.SubmittedEvents[i];

            result[i].SubmissionId.Should().Be(submittedEvent.SubmissionId);
            result[i].FileId.Should().Be(submittedEvent.FileId);
            result[i].FileName.Should().Be(submittedEvent.FileName);
            result[i].UserName.Should().Be(submittedEvent.SubmittedBy);
            result[i].SubmissionDate.Should().Be(submittedEvent.Created);
            result[i].Status.Should().Be("Submitted");
            result[i].DateofLatestStatusChange.Should().Be(submittedEvent.Created);
        }
    }

    [TestMethod]
    public async Task GetSubmissionPeriodHistory_WithDecisions_PopulatesStatus()
    {
        // Arrange
        const string queryString = "?key=value";
        var submissionId = Guid.NewGuid();
        var submittedEvents = _fixture.Build<SubmittedEventResponse>()
            .CreateMany(2)
            .ToList();

        var submissionHistoryEventsResponse = new SubmissionHistoryEventsResponse
        {
            SubmittedEvents = submittedEvents,
            RegulatorDecisionEvents = new List<RegulatorDecisionEventResponse>
        {
            new RegulatorDecisionEventResponse
            {
                SubmissionId = submittedEvents[0].SubmissionId,
                FileId = submittedEvents[0].FileId,
                Created = submittedEvents[0].Created.AddHours(2),
                Decision = "Accepted"
            },
            new RegulatorDecisionEventResponse
            {
                SubmissionId = submittedEvents[1].SubmissionId,
                FileId = submittedEvents[1].FileId,
                Created = submittedEvents[1].Created.AddDays(3),
                Decision = "Rejected"
            }
        },
            AntivirusCheckEvents = new List<AntivirusCheckEventResponse>()
        };

        _submissionStatusClientMock
            .Setup(x => x.GetSubmissionPeriodHistory(submissionId, queryString))
            .ReturnsAsync(submissionHistoryEventsResponse);

        // Act
        var result = await _systemUnderTest.GetSubmissionPeriodHistory(submissionId, queryString);

        // Assert
        result.Should().NotBeNull().And.HaveCount(2);

        for (int i = 0; i < result.Count; i++)
        {
            var submittedEvent = submissionHistoryEventsResponse.SubmittedEvents[i];
            var regulatorDecisionEvent = submissionHistoryEventsResponse.RegulatorDecisionEvents[i];

            result[i].SubmissionId.Should().Be(submittedEvent.SubmissionId);
            result[i].FileId.Should().Be(submittedEvent.FileId);
            result[i].FileName.Should().Be(submittedEvent.FileName);
            result[i].UserName.Should().Be(submittedEvent.SubmittedBy);
            result[i].SubmissionDate.Should().Be(submittedEvent.Created);
            result[i].Status.Should().Be(regulatorDecisionEvent.Decision);
            result[i].DateofLatestStatusChange.Should().Be(regulatorDecisionEvent.Created);
        }
    }

    [TestMethod]
    public async Task GetSubmissionPeriodHistory_WithMultipleDecisions_PopulatesStatusWithLatest()
    {
        // Arrange
        const string queryString = "?key=value";
        var submissionId = Guid.NewGuid();
        var submittedEvents = _fixture.Build<SubmittedEventResponse>()
            .CreateMany(1)
            .ToList();

        var submissionHistoryEventsResponse = new SubmissionHistoryEventsResponse
        {
            SubmittedEvents = submittedEvents,
            RegulatorDecisionEvents = new List<RegulatorDecisionEventResponse>
        {
            new RegulatorDecisionEventResponse
            {
                SubmissionId = submittedEvents[0].SubmissionId,
                FileId = submittedEvents[0].FileId,
                Created = submittedEvents[0].Created.AddHours(2),
                Decision = "Rejected"
            },
            new RegulatorDecisionEventResponse
            {
                SubmissionId = submittedEvents[0].SubmissionId,
                FileId = submittedEvents[0].FileId,
                Created = submittedEvents[0].Created.AddDays(3),
                Decision = "Accepted"
            }
        },
            AntivirusCheckEvents = new List<AntivirusCheckEventResponse>()
        };

        _submissionStatusClientMock
            .Setup(x => x.GetSubmissionPeriodHistory(submissionId, queryString))
            .ReturnsAsync(submissionHistoryEventsResponse);

        // Act
        var result = await _systemUnderTest.GetSubmissionPeriodHistory(submissionId, queryString);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result[0].SubmissionId.Should().Be(submissionHistoryEventsResponse.SubmittedEvents[0].SubmissionId);
        result[0].FileId.Should().Be(submissionHistoryEventsResponse.SubmittedEvents[0].FileId);
        result[0].FileName.Should().Be(submissionHistoryEventsResponse.SubmittedEvents[0].FileName);
        result[0].UserName.Should().Be(submissionHistoryEventsResponse.SubmittedEvents[0].SubmittedBy);
        result[0].SubmissionDate.Should().Be(submissionHistoryEventsResponse.SubmittedEvents[0].Created);
        result[0].Status.Should().Be("Accepted");
        result[0].DateofLatestStatusChange.Should().Be(submissionHistoryEventsResponse.RegulatorDecisionEvents[1].Created);
    }

    [TestMethod]
    public async Task GetSubmissionsByFilter_WithComplianceSchemaId_ReturnMultipleRecords()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var complianceSchemaId = Guid.NewGuid();
        var year = 2024;
        var submissionType = SubmissionType.Producer;

        var submissionsResponse = _fixture.Build<SubmissionGetResponse>()
            .CreateMany(3)
            .ToList();

        _submissionStatusClientMock
            .Setup(x => x.GetSubmissionsByFilter(organisationId, complianceSchemaId, year, submissionType))
            .ReturnsAsync(submissionsResponse);

        // Act
        var result = await _systemUnderTest.GetSubmissionsByFilter(organisationId, complianceSchemaId, year, submissionType);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(3);
    }

    [TestMethod]
    public async Task GetSubmissionsByFilter_WithOutComplianceSchemaId_ReturnMultipleRecords()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var year = 2024;
        var submissionType = SubmissionType.Producer;

        var submissionsResponse = _fixture.Build<SubmissionGetResponse>()
            .CreateMany(3)
            .ToList();

        _submissionStatusClientMock
            .Setup(x => x.GetSubmissionsByFilter(organisationId, Guid.Empty, year, submissionType))
            .ReturnsAsync(submissionsResponse);

        // Act
        var result = await _systemUnderTest.GetSubmissionsByFilter(organisationId, Guid.Empty, year, submissionType);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(3);
    }

    [TestMethod]
    public async Task GetSubmissionsByFilter_WithoutYear_ReturnMultipleRecords()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var complianceSchemaId = Guid.NewGuid();
        var submissionType = SubmissionType.Producer;

        var submissionsResponse = _fixture.Build<SubmissionGetResponse>()
            .CreateMany(3)
            .ToList();

        _submissionStatusClientMock
            .Setup(x => x.GetSubmissionsByFilter(organisationId, complianceSchemaId, null, submissionType))
            .ReturnsAsync(submissionsResponse);

        // Act
        var result = await _systemUnderTest.GetSubmissionsByFilter(organisationId, complianceSchemaId, null, submissionType);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(3);
    }

    [TestMethod]
    public async Task GetRegistrationValidationErrorsAsync_ReturnsRegistrationValidationErrorsRows()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var registrationValidationErrorsRows = GenerateRandomRegistrationValidationIssueList().ToList();

        _submissionStatusClientMock.Setup(x => x.GetRegistrationValidationErrorsAsync(submissionId)).ReturnsAsync(registrationValidationErrorsRows);

        // Act
        var result = await _systemUnderTest.GetRegistrationValidationErrorsAsync(submissionId);

        // Assert
        result.Should().BeEquivalentTo(registrationValidationErrorsRows);

        _submissionStatusClientMock.Verify(x => x.GetRegistrationValidationErrorsAsync(submissionId), Times.Once);
    }

    private static IEnumerable<ProducerValidationIssueRow> GenerateRandomProducerValidationIssueList()
    {
        var random = new Random();
        return _fixture
            .Build<ProducerValidationIssueRow>()
            .With(x => x.RowNumber, random.Next(1, 20))
            .CreateMany(10)
            .ToList();
    }

    private static IEnumerable<RegistrationValidationError> GenerateRandomRegistrationValidationIssueList()
    {
        var random = new Random();
        return _fixture
            .Build<RegistrationValidationError>()
            .CreateMany(10)
            .ToList();
    }
}
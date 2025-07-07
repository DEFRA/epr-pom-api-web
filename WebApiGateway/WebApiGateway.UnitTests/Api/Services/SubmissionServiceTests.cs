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
using WebApiGateway.Api.Constants;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Constants;
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
    private const string RegistrationContainerName = "registration-container-name";
    private const string PomContainerName = "pom-container-name";
    private const string SubsidiaryContainerName = "subsidiary-container-name";
    private const string AccreditationContainerName = "accreditation-container-name";

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
                PomContainer = PomContainerName,
                SubsidiaryContainer = SubsidiaryContainerName,
                AccreditationContainer = AccreditationContainerName,
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
        await _systemUnderTest.CreateSubmissionAsync(SubmissionType, SubmissionPeriod, null, true);

        // Assert
        _submissionStatusClientMock.Verify(
            x => x.CreateSubmissionAsync(
                It.Is<CreateSubmission>(m =>
                    m.SubmissionType == SubmissionType
                    && m.SubmissionPeriod == SubmissionPeriod
                    && m.DataSourceType == DataSourceType.File
                    && m.Id != Guid.Empty
                    && m.IsResubmission == true)),
            Times.Once);
    }

    [TestMethod]
    [DataRow(FileType.Pom, PomContainerName)]
    [DataRow(FileType.Brands, RegistrationContainerName)]
    [DataRow(FileType.Subsidiaries, SubsidiaryContainerName)]
    [DataRow(FileType.Accreditation, AccreditationContainerName)]
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
                    && m.BlobContainerName == PomContainerName
                    && m.FileId != Guid.Empty),
                submissionId),
            Times.Once);

        _loggingServiceMock.Verify(x => x.SendEventAsync(It.IsAny<ProtectiveMonitoringEvent>()), Times.Once);
        _loggerMock.VerifyLog(x => x.LogError(httpRequestException, "An error occurred creating the protective monitoring event"));
    }

    [TestMethod]
    public async Task When_Invalid_SubmissionType_Does_Nothing()
    {
        // Arrange
        const string Comments = "Pay part-payment of £24,500 now";
        const string AppReferenceNumber = "PEPR00002125P1";
        var submissionId = Guid.NewGuid();
        var complianceSchemeId = Guid.NewGuid();

        // Act
        await _systemUnderTest.CreateRegistrationEventAsync(submissionId, new RegistrationApplicationPayload
        {
            SubmissionType = SubmissionType.Producer,
            ApplicationReferenceNumber = AppReferenceNumber,
            Comments = Comments,
            ComplianceSchemeId = complianceSchemeId
        });

        // Assert
        _submissionStatusClientMock.Verify(
            x => x.CreateApplicationSubmittedEventAsync(
                It.IsAny<RegistrationApplicationSubmittedEvent>(),
                submissionId),
            Times.Never);

        _submissionStatusClientMock.Verify(
            x => x.CreateRegistrationFeePaymentEventAsync(
                It.IsAny<RegistrationFeePaymentEvent>(),
                submissionId),
            Times.Never);
    }

    [TestMethod]
    public async Task WhenRegistrationFeePaymentEventGenerated_CreateRegistrationFeePaymentEventAsync_CompletesSuccessfully()
    {
        // Arrange
        const string PaidAmount = "123";
        const string PaymentMethod = "PayByPhone";
        const string PaymentStatus = "done";
        const string AppReferenceNumber = "PEPR00002125P1";
        var submissionId = Guid.NewGuid();

        // Act
        await _systemUnderTest.CreateRegistrationEventAsync(submissionId, new RegistrationApplicationPayload
        {
            SubmissionType = SubmissionType.RegistrationFeePayment,
            ApplicationReferenceNumber = AppReferenceNumber,
            PaymentMethod = PaymentMethod,
            PaidAmount = PaidAmount,
            PaymentStatus = PaymentStatus
        });

        // Assert
        _submissionStatusClientMock.Verify(
            x => x.CreateRegistrationFeePaymentEventAsync(
                It.Is<RegistrationFeePaymentEvent>(m =>
                    m.PaymentMethod == PaymentMethod &&
                    m.ApplicationReferenceNumber == AppReferenceNumber &&
                    m.PaymentStatus == PaymentStatus &&
                    m.PaymentMethod == PaymentMethod),
                submissionId),
            Times.Once);
    }

    [TestMethod]
    public async Task CreateRegistrationEventAsync_ShouldCreateRegistrationFeePaymentEvent_WhenSubmissionTypeIsRegistrationFeePayment()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var applicationPayload = Fixture.Build<RegistrationApplicationPayload>()
            .With(p => p.SubmissionType, SubmissionType.RegistrationFeePayment)
            .Create();

        // Act
        await _systemUnderTest.CreateRegistrationEventAsync(submissionId, applicationPayload);

        // Assert
        _submissionStatusClientMock.Verify(client => client.CreateRegistrationFeePaymentEventAsync(It.IsAny<RegistrationFeePaymentEvent>(), submissionId), Times.Once);
        _submissionStatusClientMock.Verify(client => client.CreateApplicationSubmittedEventAsync(It.IsAny<RegistrationApplicationSubmittedEvent>(), submissionId), Times.Never);
    }

    [TestMethod]
    public async Task CreateRegistrationEventAsync_ShouldCreateRegistrationApplicationSubmittedEvent_WhenSubmissionTypeIsNotRegistrationFeePayment()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var applicationPayload = Fixture.Build<RegistrationApplicationPayload>()
            .With(p => p.SubmissionType, SubmissionType.RegistrationApplicationSubmitted)
            .Create();

        // Act
        await _systemUnderTest.CreateRegistrationEventAsync(submissionId, applicationPayload);

        // Assert
        _submissionStatusClientMock.Verify(client => client.CreateRegistrationFeePaymentEventAsync(It.IsAny<RegistrationFeePaymentEvent>(), submissionId), Times.Never);
        _submissionStatusClientMock.Verify(client => client.CreateApplicationSubmittedEventAsync(It.IsAny<RegistrationApplicationSubmittedEvent>(), submissionId), Times.Once);
    }

    [TestMethod]
    public async Task CreateRegistrationEventAsync_ShouldSetCorrectFeePaymentEventFields_WhenSubmissionTypeIsRegistrationFeePayment()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var applicationPayload = Fixture.Build<RegistrationApplicationPayload>()
            .With(p => p.SubmissionType, SubmissionType.RegistrationFeePayment)
            .With(p => p.ApplicationReferenceNumber, "REF123")
            .With(p => p.PaidAmount, "100.00m")
            .With(p => p.PaymentMethod, "Credit Card")
            .With(p => p.PaymentStatus, "Paid")
            .Create();

        // Act
        await _systemUnderTest.CreateRegistrationEventAsync(submissionId, applicationPayload);

        // Assert
        _submissionStatusClientMock.Verify(client => client.CreateRegistrationFeePaymentEventAsync(It.Is<RegistrationFeePaymentEvent>(eventArg => eventArg.ApplicationReferenceNumber == "REF123" && eventArg.PaidAmount == "100.00m" && eventArg.PaymentMethod == "Credit Card" && eventArg.PaymentStatus == "Paid"), submissionId), Times.Once);
    }

    [TestMethod]
    public async Task CreateRegistrationEventAsync_ShouldSetCorrectApplicationSubmittedEventFields_WhenSubmissionTypeIsRegistrationApplicationSubmitted()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var applicationPayload = Fixture.Build<RegistrationApplicationPayload>()
            .With(p => p.SubmissionType, SubmissionType.RegistrationApplicationSubmitted)
            .With(p => p.Comments, "Application submitted successfully")
            .With(p => p.ApplicationReferenceNumber, "APP123")
            .Create();

        // Act
        await _systemUnderTest.CreateRegistrationEventAsync(submissionId, applicationPayload);

        // Assert
        _submissionStatusClientMock.Verify(client => client.CreateApplicationSubmittedEventAsync(It.Is<RegistrationApplicationSubmittedEvent>(eventArg => eventArg.Comments == "Application submitted successfully" && eventArg.ApplicationReferenceNumber == "APP123" && eventArg.SubmissionDate.Value.Date == DateTime.Today), submissionId), Times.Once);
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
        var submissions = Fixture.Create<List<AbstractSubmission>>();
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
        const string QueryString = "?key=value";
        var submissionId = Guid.NewGuid();
        var submissionHistoryEventsResponse = new SubmissionHistoryEventsResponse
        {
            SubmittedEvents = Fixture.Build<SubmittedEventResponse>()
            .CreateMany(2)
            .ToList(),
            RegulatorDecisionEvents = new List<RegulatorDecisionEventResponse>(),
            AntivirusCheckEvents = new List<AntivirusCheckEventResponse>()
        };

        _submissionStatusClientMock.Setup(x => x.GetSubmissionPeriodHistory(submissionId, QueryString)).ReturnsAsync(submissionHistoryEventsResponse);

        // Act
        var result = await _systemUnderTest.GetSubmissionPeriodHistory(submissionId, QueryString);

        // Assert
        result.Should().NotBeNull().And.HaveCount(2);

        for (var i = 0; i < result.Count; i++)
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
        const string QueryString = "?key=value";
        var submissionId = Guid.NewGuid();
        var submittedEvents = Fixture.Build<SubmittedEventResponse>()
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
            .Setup(x => x.GetSubmissionPeriodHistory(submissionId, QueryString))
            .ReturnsAsync(submissionHistoryEventsResponse);

        // Act
        var result = await _systemUnderTest.GetSubmissionPeriodHistory(submissionId, QueryString);

        // Assert
        result.Should().NotBeNull().And.HaveCount(2);

        for (var i = 0; i < result.Count; i++)
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
        const string QueryString = "?key=value";
        var submissionId = Guid.NewGuid();
        var submittedEvents = Fixture.Build<SubmittedEventResponse>()
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
            .Setup(x => x.GetSubmissionPeriodHistory(submissionId, QueryString))
            .ReturnsAsync(submissionHistoryEventsResponse);

        // Act
        var result = await _systemUnderTest.GetSubmissionPeriodHistory(submissionId, QueryString);

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
        var organisationId = Guid.NewGuid();
        var complianceSchemaId = Guid.NewGuid();
        var year = 2024;
        var submissionType = SubmissionType.Producer;

        var submissionsResponse = Fixture.Build<SubmissionGetResponse>()
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
        var organisationId = Guid.NewGuid();
        var year = 2024;
        var submissionType = SubmissionType.Producer;

        var submissionsResponse = Fixture.Build<SubmissionGetResponse>()
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
        var organisationId = Guid.NewGuid();
        var complianceSchemaId = Guid.NewGuid();
        var submissionType = SubmissionType.Producer;

        var submissionsResponse = Fixture.Build<SubmissionGetResponse>()
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
    public async Task GetRegistrationValidationIssuesAsync_ReturnsRegistrationValidationErrors_And_WarningsRows()
    {
        // Arrange
        var submissionId = Guid.NewGuid();

        var registrationValidationErrorsRows = GenerateRandomRegistrationValidationErrorList().ToList();
        _submissionStatusClientMock.Setup(x => x.GetRegistrationValidationErrorsAsync(submissionId)).ReturnsAsync(registrationValidationErrorsRows);

        var warningRows = GenerateRandomRegistrationValidationWarningList().ToList();
        _submissionStatusClientMock.Setup(x => x.GetRegistrationValidationWarningsAsync(submissionId)).ReturnsAsync(warningRows);

        var resultedList = registrationValidationErrorsRows.Concat(warningRows).OrderBy(x => x.RowNumber).ToList();

        // Act
        var result = await _systemUnderTest.GetRegistrationValidationErrorsAsync(submissionId);

        // Assert
        result.Should().BeEquivalentTo(resultedList);

        _submissionStatusClientMock.Verify(x => x.GetRegistrationValidationErrorsAsync(submissionId), Times.Once);
        _submissionStatusClientMock.Verify(x => x.GetRegistrationValidationWarningsAsync(submissionId), Times.Once);
    }

    [TestMethod]
    [DataRow(ContentScan.Clean)]
    [DataRow(ContentScan.Malicious)]

    public async Task CreateFileDownloadCheckEventAsync_LogsCorrectTransactionCode(string contentScan)
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var filename = "filename.csv";

        var fileDownloadCheckEvent = new FileDownloadCheckEvent()
        {
            ContentScan = contentScan,
            FileId = fileId,
            FileName = filename,
            BlobName = fileId.ToString(),
            SubmissionType = SubmissionType.Registration
        };

        // Act
        await _systemUnderTest.CreateFileDownloadCheckEventAsync(sessionId, fileDownloadCheckEvent);

        // Assert
        _submissionStatusClientMock.Verify(
            x => x.CreateFileDownloadEventAsync(
                It.Is<FileDownloadCheckEvent>(m =>
                    m.ContentScan == contentScan
                    && m.FileId != Guid.Empty
                    && m.FileName == filename
                    && m.BlobName == fileId.ToString()
                    && m.SubmissionType == SubmissionType.Registration),
                sessionId),
            Times.Once);

        var expectedTransactionCode = contentScan == ContentScan.Clean ? TransactionCodes.DownloadAllowed : TransactionCodes.AntivirusThreatDetected;

        _loggingServiceMock.Verify(
            x => x.SendEventAsync(
                It.Is<ProtectiveMonitoringEvent>(m =>
                    m.SessionId == sessionId
                    && m.Component == "epr_pom_api_web"
                    && m.PmcCode == PmcCodes.Code0212
                    && m.Priority == 0
                    && m.TransactionCode == expectedTransactionCode
                    && m.Message == filename
                    && m.AdditionalInfo == $"FileId: {fileId}")),
            Times.Once);
    }

    [TestMethod]
    public async Task CreateFileDownloadCheckEventAsync_ThrowsException_WhenLoggingEvent()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var filename = "filename.csv";

        var fileDownloadCheckEvent = new FileDownloadCheckEvent()
        {
            ContentScan = ContentScan.Clean,
            FileId = fileId,
            FileName = filename,
            BlobName = fileId.ToString(),
            SubmissionType = SubmissionType.Registration
        };

        var httpRequestException = new HttpRequestException();

        _loggingServiceMock.Setup(x => x.SendEventAsync(It.IsAny<ProtectiveMonitoringEvent>())).ThrowsAsync(httpRequestException);

        // Act
        await _systemUnderTest.CreateFileDownloadCheckEventAsync(sessionId, fileDownloadCheckEvent);

        // Assert
        _submissionStatusClientMock.Verify(
            x => x.CreateFileDownloadEventAsync(
                It.Is<FileDownloadCheckEvent>(m =>
                    m.ContentScan == ContentScan.Clean
                    && m.FileId != Guid.Empty
                    && m.FileName == filename
                    && m.BlobName == fileId.ToString()
                    && m.SubmissionType == SubmissionType.Registration),
                sessionId),
            Times.Once);

        _loggingServiceMock.Verify(x => x.SendEventAsync(It.IsAny<ProtectiveMonitoringEvent>()), Times.Once);
        _loggerMock.VerifyLog(x => x.LogError(httpRequestException, "An error occurred creating the protective monitoring event"));
    }

    [TestMethod]
    public async Task GetFileBlobNameAsync_ReturnsBlobName()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var blobName = Guid.NewGuid().ToString();
        var antivirusResultEvent = new AntivirusResultEvent
        {
            FileId = fileId,
            BlobName = blobName,
            SubmissionType = SubmissionType.Registration,
            SubmissionId = Guid.NewGuid(),
            AntivirusScanResult = AntivirusScanResult.Success,
            AntivirusScanTrigger = AntivirusScanTrigger.Download,
            UserId = Guid.NewGuid(),
            OrganisationId = Guid.NewGuid(),
            Errors = new List<string>()
        };

        _submissionStatusClientMock.Setup(x => x.GetFileScanResultAsync(submissionId, fileId)).ReturnsAsync(antivirusResultEvent);

        // Act
        var result = await _systemUnderTest.GetFileBlobNameAsync(submissionId, fileId);

        // Assert
        result.Should().Be(blobName);
    }

    [TestMethod]
    public async Task GetFileBlobNameAsync_ThrowsException_WhenGetFileScanResultAsyncFails()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var exception = new Exception();
        _submissionStatusClientMock.Setup(x => x.GetFileScanResultAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ThrowsAsync(exception);

        // Act
        Func<Task> act = async () => await _systemUnderTest.GetFileBlobNameAsync(submissionId, fileId);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    private static List<ProducerValidationIssueRow> GenerateRandomProducerValidationIssueList()
    {
        var random = new Random();
        return Fixture
            .Build<ProducerValidationIssueRow>()
            .With(x => x.RowNumber, random.Next(1, 20))
            .CreateMany(10)
            .ToList();
    }

    private static List<RegistrationValidationError> GenerateRandomRegistrationValidationErrorList()
    {
        var fixture = Fixture.Build<RegistrationValidationError>()
                                .CreateMany(10)
                                .ToList();

        fixture.ForEach(x => x.IssueType = IssueType.Error);
        return fixture;
    }

    private static List<RegistrationValidationError> GenerateRandomRegistrationValidationWarningList()
    {
        var fixture = Fixture.Build<RegistrationValidationError>()
                                .CreateMany(2)
                                .ToList();

        fixture.ForEach(x => x.IssueType = IssueType.Warning);
        return fixture;
    }
}
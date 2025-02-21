using System.Net;
using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Controllers;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.ProducerValidation;
using WebApiGateway.Core.Models.RegistrationValidation;
using WebApiGateway.Core.Models.Submission;
using WebApiGateway.Core.Models.SubmissionHistory;
using WebApiGateway.Core.Models.Submissions;
using WebApiGateway.UnitTests.Support.Extensions;

namespace WebApiGateway.UnitTests.Api.Controllers;

[TestClass]
public class SubmissionControllerTests
{
    private static readonly IFixture Fixture = new Fixture().Customize(new AutoMoqCustomization());
    private Mock<ISubmissionService> _submissionServiceMock;
    private SubmissionController _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _submissionServiceMock = new Mock<ISubmissionService>();
        _systemUnderTest = new SubmissionController(_submissionServiceMock.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
    }

    [TestMethod]
    public async Task GetSubmission_ReturnsCorrectResponse()
    {
        // Arrange
        var submission = new PomSubmission { Id = Guid.NewGuid() };
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = submission.ToJsonContent()
        };
        _submissionServiceMock.Setup(x => x.GetSubmissionAsync(submission.Id)).ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _systemUnderTest.GetSubmission(submission.Id) as ContentResult;

        // Assert
        result.Content.Should().Be(JsonSerializer.Serialize(submission));
        result.StatusCode.Should().Be((int)httpResponseMessage.StatusCode);
        result.ContentType.Should().Be("application/json");

        _submissionServiceMock.Verify(x => x.GetSubmissionAsync(submission.Id), Times.Once);
    }

    [TestMethod]
    public async Task GetSubmissions_ReturnsOkObjectResult()
    {
        // Arrange
        const string QueryString = "?key=value";
        _systemUnderTest.HttpContext.Request.QueryString = new QueryString(QueryString);
        var submissions = Fixture.Create<List<AbstractSubmission>>();

        _submissionServiceMock.Setup(x => x.GetSubmissionsAsync(QueryString)).ReturnsAsync(submissions);

        // Act
        var result = await _systemUnderTest.GetSubmissions() as OkObjectResult;

        // Assert
        result.Value.Should().BeEquivalentTo(submissions);
        _submissionServiceMock.Verify(x => x.GetSubmissionsAsync(QueryString), Times.Once);
    }

    [TestMethod]
    public async Task GetProducerValidationIssues_ReturnsOkObjectResult()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var validationIssueRows = Fixture.Create<List<ProducerValidationIssueRow>>();
        _submissionServiceMock.Setup(x => x.GetProducerValidationIssuesAsync(submissionId)).ReturnsAsync(validationIssueRows);

        // Act
        var result = await _systemUnderTest.GetProducerValidationIssues(submissionId) as OkObjectResult;

        // Assert
        result.Value.Should().Be(validationIssueRows);
        _submissionServiceMock.Verify(x => x.GetProducerValidationIssuesAsync(submissionId), Times.Once);
    }

    [TestMethod]
    public async Task Submit_ReturnsNoContentResult()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var submissionPayload = new SubmissionPayload
        {
            SubmittedBy = "Test Name",
            FileId = Guid.NewGuid()
        };

        // Act
        var result = await _systemUnderTest.Submit(submissionId, submissionPayload);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _submissionServiceMock.Verify(x => x.SubmitAsync(submissionId, submissionPayload), Times.Once);
    }

    [TestMethod]
    public async Task SubmitIsCalled_ReturnsNoContentResult()
    {
        // Arrange
        var submission = new CreateSubmission
        {
            Id = Guid.NewGuid(),
            SubmissionType = SubmissionType.Registration,
            AppReferenceNumber = "PEPR00002125P1",
        };

        // Act
        var result = await _systemUnderTest.Submit(submission);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _submissionServiceMock.Verify(x => x.SubmitAsync(submission), Times.Once);
    }

    [TestMethod]
    public async Task SubmitRegistrationApplicationIsCalled_ReturnsNoContentResult()
    {
        // Arrange
        Guid submissionId = Guid.NewGuid();
        var applicationPayload = new RegistrationApplicationPayload
        { Comments = "We've agreed to settle for a part-payment of £24,500 now.", ApplicationReferenceNumber = "PEPR00002125P1" };

        // Act
        var result = await _systemUnderTest.SubmitRegistrationApplication(submissionId, applicationPayload);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _submissionServiceMock.Verify(x => x.CreateRegistrationEventAsync(submissionId, It.IsAny<RegistrationApplicationPayload>()), Times.Once);
    }

    [TestMethod]
    public async Task GetSubmissionHistory_ReturnsOkObjectResult()
    {
        // Arrange
        const string QueryString = "?key=value";
        var submissionId = Guid.NewGuid();
        _systemUnderTest.HttpContext.Request.QueryString = new QueryString(QueryString);
        var submissions = Fixture.Create<List<SubmissionHistoryResponse>>();

        _submissionServiceMock.Setup(x => x.GetSubmissionPeriodHistory(submissionId, QueryString)).ReturnsAsync(submissions);

        // Act
        var result = await _systemUnderTest.GetSubmissionHistory(submissionId) as OkObjectResult;

        // Assert
        result.Value.Should().BeEquivalentTo(submissions);
        _submissionServiceMock.Verify(x => x.GetSubmissionPeriodHistory(submissionId, QueryString), Times.Once);
    }

    [TestMethod]
    public async Task GetSubmissionByFilter_ReturnsOkObjectResult()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var complianceSchemaId = Guid.NewGuid();
        var year = 2024;
        var submissionType = SubmissionType.Producer;

        var submissions = Fixture.Create<List<SubmissionGetResponse>>();

        _submissionServiceMock.Setup(x =>
            x.GetSubmissionsByFilter(
            organisationId,
            complianceSchemaId,
            year,
            submissionType))
            .ReturnsAsync(submissions);

        // Act
        var result = await _systemUnderTest.GetSubmissionByFilter(organisationId, new SubmissionGetRequest
        {
            ComplianceSchemeId = complianceSchemaId,
            Type = submissionType,
            Year = year
        }) as OkObjectResult;

        // Assert
        result.Value.Should().BeEquivalentTo(submissions);

        _submissionServiceMock.Verify(
            x => x.GetSubmissionsByFilter(
            organisationId,
            complianceSchemaId,
            year,
            submissionType),
            Times.Once);
    }

    [TestMethod]
    public async Task GetRegistrationValidationErrors_ReturnsOkObjectResult()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var validationIssueRows = Fixture.Create<List<RegistrationValidationError>>();
        _submissionServiceMock.Setup(x => x.GetRegistrationValidationErrorsAsync(submissionId)).ReturnsAsync(validationIssueRows);

        // Act
        var result = await _systemUnderTest.GetRegistrationValidationErrors(submissionId) as OkObjectResult;

        // Assert
        result.Value.Should().Be(validationIssueRows);
        _submissionServiceMock.Verify(x => x.GetRegistrationValidationErrorsAsync(submissionId), Times.Once);
    }
}
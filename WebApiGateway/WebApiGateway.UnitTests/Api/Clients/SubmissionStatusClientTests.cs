using System.Net;
using System.Net.Http.Json;
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
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.ProducerValidation;
using WebApiGateway.Core.Models.RegistrationValidation;
using WebApiGateway.Core.Models.Submission;
using WebApiGateway.Core.Models.SubmissionHistory;
using WebApiGateway.Core.Models.Submissions;
using WebApiGateway.Core.Models.UserAccount;
using WebApiGateway.UnitTests.Support.Extensions;

namespace WebApiGateway.UnitTests.Api.Clients;

[TestClass]
public class SubmissionStatusClientTests
{
    private static readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly UserAccount _userAccount = _fixture.Create<UserAccount>();

    private Mock<ILogger<SubmissionStatusClient>> _loggerMock;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private Mock<IAccountServiceClient> _accountServiceClientMock;
    private ISubmissionStatusClient _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
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

        _accountServiceClientMock = new Mock<IAccountServiceClient>();
        _accountServiceClientMock.Setup(x => x.GetUserAccount(_userAccount.User.Id)).ReturnsAsync(_userAccount);
        httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipalMock.Object);
        _systemUnderTest = new SubmissionStatusClient(httpClient, _accountServiceClientMock.Object, httpContextAccessorMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task CreateEventAsync_DoesNotThrowException_WhenHttpClientResponseIsCreated()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var antivirusCheckEvent = _fixture.Create<AntivirusCheckEvent>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.Created, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.CreateEventAsync(antivirusCheckEvent, submissionId))
            .Should()
            .NotThrowAsync();

        var expectedMethod = HttpMethod.Post;
        var expectedRequestUri = new Uri($"https://example.com/submissions/{submissionId}/events");
        var expectedHeaders = new Dictionary<string, string>
        {
            { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
            { "UserId", _userAccount.User.Id.ToString() }
        };

        _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, expectedHeaders, Times.Once());
    }

    [TestMethod]
    public async Task CreateEventAsync_LogsAndThrowsException_WhenHttpClientResponseIsInternalServerError()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var antivirusCheckEvent = _fixture.Create<AntivirusCheckEvent>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.CreateEventAsync(antivirusCheckEvent, submissionId))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError("Error creating AntivirusCheck event"));
    }

    [TestMethod]
    public async Task CreateSubmissionAsync_DoesNotThrowException_WhenHttpClientResponseIsCreated()
    {
        // Arrange
        var createSubmission = _fixture.Create<CreateSubmission>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.Created, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.CreateSubmissionAsync(createSubmission))
            .Should()
            .NotThrowAsync();

        var expectedMethod = HttpMethod.Post;
        var expectedRequestUri = new Uri($"https://example.com/submissions");
        var expectedHeaders = new Dictionary<string, string>
        {
            { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
            { "UserId", _userAccount.User.Id.ToString() }
        };

        _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, expectedHeaders, Times.Once());
    }

    [TestMethod]
    public async Task CreateSubmissionAsync_LogsAndThrowsException_WhenHttpClientResponseIsInternalServerError()
    {
        // Arrange
        var createSubmission = _fixture.Create<CreateSubmission>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.CreateSubmissionAsync(createSubmission))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError("Error creating submission"));
    }

    [TestMethod]
    public async Task GetSubmissionAsync_ReturnsHttpResponseMessage()
    {
        // Arrange
        var submission = _fixture.Create<RegistrationSubmission>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, submission.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetSubmissionAsync(submission.Id);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        (await result.Content.ReadFromJsonAsync<RegistrationSubmission>()).Should().BeEquivalentTo(submission);

        var expectedMethod = HttpMethod.Get;
        var expectedRequestUri = new Uri($"https://example.com/submissions/{submission.Id}");
        var expectedHeaders = new Dictionary<string, string>
        {
            { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
            { "UserId", _userAccount.User.Id.ToString() }
        };

        _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, expectedHeaders, Times.Once());
    }

    [TestMethod]
    public async Task GetSubmissionsAsync_ReturnsSubmissions_WhenHttpClientResponseIsOk()
    {
        // Arrange
        const string QueryString = "?key=value";
        var submissions = _fixture.Create<List<RegistrationSubmission>>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, submissions.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetSubmissionsAsync(QueryString);

        // Assert
        result.Should().BeEquivalentTo(submissions);

        var expectedMethod = HttpMethod.Get;
        var expectedRequestUri = new Uri($"https://example.com/submissions{QueryString}");
        var expectedHeaders = new Dictionary<string, string>
        {
            { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
            { "UserId", _userAccount.User.Id.ToString() }
        };

        _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, expectedHeaders, Times.Once());
    }

    [TestMethod]
    public async Task GetSubmissionsAsync_LogsAndThrowsException_WhenHttpClientResponseIsInternalServerError()
    {
        // Arrange
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.GetSubmissionsAsync(string.Empty))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error getting submissions"));
    }

    [TestMethod]
    public async Task GetProducerValidationErrorRowsAsync_ReturnsSubmissions_WhenHttpClientResponseIsOk()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var validationErrorRows = _fixture.Build<ProducerValidationIssueRow>()
            .With(x => x.Issue, "Error")
            .CreateMany()
            .ToList();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, validationErrorRows.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetProducerValidationErrorRowsAsync(submissionId);

        // Assert
        result.Should().BeEquivalentTo(validationErrorRows);

        var expectedMethod = HttpMethod.Get;
        var expectedRequestUri = new Uri($"https://example.com/submissions/{submissionId}/producer-validations");
        var expectedHeaders = new Dictionary<string, string>
        {
            { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
            { "UserId", _userAccount.User.Id.ToString() }
        };

        _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, expectedHeaders, Times.Once());
    }

    [TestMethod]
    public async Task GetProducerValidationErrorRowsAsync_LogsAndThrowsException_WhenHttpClientResponseIsInternalServerError()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.GetProducerValidationErrorRowsAsync(submissionId))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error getting producer validation errors"));
    }

    [TestMethod]
    public async Task GetProducerValidationWarningRowsAsync_ReturnsSubmissions_WhenHttpClientResponseIsOk()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var validationWarningRows = _fixture.Build<ProducerValidationIssueRow>()
            .With(x => x.Issue, "Warning")
            .CreateMany()
            .ToList();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, validationWarningRows.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetProducerValidationWarningRowsAsync(submissionId);

        // Assert
        result.Should().BeEquivalentTo(validationWarningRows);

        var expectedMethod = HttpMethod.Get;
        var expectedRequestUri = new Uri($"https://example.com/submissions/{submissionId}/producer-warning-validations");
        var expectedHeaders = new Dictionary<string, string>
        {
            { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
            { "UserId", _userAccount.User.Id.ToString() }
        };

        _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, expectedHeaders, Times.Once());
    }

    [TestMethod]
    public async Task GetProducerValidationWarningRowsAsync_LogsAndThrowsException_WhenHttpClientResponseIsInternalServerError()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.GetProducerValidationWarningRowsAsync(submissionId))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error getting producer validation warnings"));
    }

    [TestMethod]
    public async Task SubmitAsync_LogsAndThrowsException_WhenHttpClientResponseIsInternalServerError()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var submissionPayload = new SubmissionPayload
        {
            SubmittedBy = "Test Name",
            FileId = fileId
        };
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.SubmitAsync(submissionId, submissionPayload))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error submitting submission with id {submissionId} and file id {fileId}", submissionId, fileId));
    }

    [TestMethod]
    public async Task SubmitAsync_CallsSubmissionApiWithCorrectRequestParameters()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var submissionPayload = new SubmissionPayload
        {
            SubmittedBy = "Test Name",
            FileId = Guid.NewGuid()
        };
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.NoContent, null);

        // Act
        await _systemUnderTest.SubmitAsync(submissionId, submissionPayload);

        // Assert
        var expectedMethod = HttpMethod.Post;
        var expectedRequestUri = new Uri($"https://example.com/submissions/{submissionId}/submit");
        var expectedHeaders = new Dictionary<string, string>
        {
            { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
            { "UserId", _userAccount.User.Id.ToString() }
        };

        _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, expectedHeaders, Times.Once());
    }

    [TestMethod]
    public async Task GetRegistrationValidationErrorsAsync_ReturnsSubmissions_WhenHttpClientResponseIsOk()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var registrationValidationErrorRows = _fixture.Build<RegistrationValidationError>()
            .CreateMany()
            .ToList();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, registrationValidationErrorRows.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetRegistrationValidationErrorsAsync(submissionId);

        // Assert
        result.Should().BeEquivalentTo(registrationValidationErrorRows);

        var expectedMethod = HttpMethod.Get;
        var expectedRequestUri = new Uri($"https://example.com/submissions/{submissionId}/organisation-details-errors");
        var expectedHeaders = new Dictionary<string, string>
        {
            { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
            { "UserId", _userAccount.User.Id.ToString() }
        };

        _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, expectedHeaders, Times.Once());
    }

    [TestMethod]
    public async Task GetSubmissionPeriodHistory_WhenSubmittedByIsPresent_ReturnsExpectedValue()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        const string QueryString = "?key=value";
        var submissionHistoryEventsResponse = new SubmissionHistoryEventsResponse
        {
            SubmittedEvents = _fixture.Build<SubmittedEventResponse>()
            .With(x => x.SubmittedBy, "First Last")
            .CreateMany()
            .ToList(),
            RegulatorDecisionEvents = _fixture.Build<RegulatorDecisionEventResponse>()
            .CreateMany()
            .ToList(),
            AntivirusCheckEvents = _fixture.Build<AntivirusCheckEventResponse>()
            .CreateMany()
            .ToList()
        };

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, submissionHistoryEventsResponse.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetSubmissionPeriodHistory(submissionId, QueryString);

        // Assert
        result.Should().BeEquivalentTo(submissionHistoryEventsResponse);

        var expectedMethod = HttpMethod.Get;
        var expectedRequestUri = new Uri($"https://example.com/submissions/events/events-by-type/{submissionId}{QueryString}");
        var expectedHeaders = new Dictionary<string, string>
        {
            { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
            { "UserId", _userAccount.User.Id.ToString() }
        };

        _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, expectedHeaders, Times.Once());
    }

    [TestMethod]
    public async Task GetSubmissionPeriodHistory_WhenSubmittedByIsNullOrMissing_CallsGetUserAccount()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var userOne = _fixture.Create<UserAccount>();
        var userTwo = _fixture.Create<UserAccount>();
        const string QueryString = "?key=value";
        var submissionHistoryEventsResponse = new SubmissionHistoryEventsResponse
        {
            SubmittedEvents = _fixture.Build<SubmittedEventResponse>()
            .Without(x => x.SubmittedBy)
            .CreateMany(2)
            .ToList(),
            RegulatorDecisionEvents = _fixture.Build<RegulatorDecisionEventResponse>()
            .CreateMany(2)
            .ToList(),
            AntivirusCheckEvents = _fixture.Build<AntivirusCheckEventResponse>()
            .CreateMany(2)
            .ToList()
        };

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, submissionHistoryEventsResponse.ToJsonContent());

        var userIdOne = submissionHistoryEventsResponse.SubmittedEvents.First().UserId;
        var userIdTwo = submissionHistoryEventsResponse.SubmittedEvents[1].UserId;

        _accountServiceClientMock.Setup(x => x.GetUserAccount(userIdOne)).ReturnsAsync(userOne);
        _accountServiceClientMock.Setup(x => x.GetUserAccount(userIdTwo)).ReturnsAsync(userTwo);

        // Act
        var result = await _systemUnderTest.GetSubmissionPeriodHistory(submissionId, QueryString);

        // Assert
        result.SubmittedEvents[0].SubmittedBy.Should().BeEquivalentTo(userOne.User.FirstName + " " + userOne.User.LastName);
        result.SubmittedEvents[1].SubmittedBy.Should().BeEquivalentTo(userTwo.User.FirstName + " " + userTwo.User.LastName);

        _accountServiceClientMock.Verify(x => x.GetUserAccount(userIdOne), Times.Once);
        _accountServiceClientMock.Verify(x => x.GetUserAccount(userIdTwo), Times.Once);
    }

    [TestMethod]
    public async Task GetSubmissionByFilter_WithComplianceSchemaId_ReturnsExpectedValue()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var complianceSchemaId = Guid.NewGuid();
        var year = 2024;
        var submissionType = SubmissionType.Producer;

        var submissions = new List<SubmissionGetResponse>
        {
            new SubmissionGetResponse
            {
                SubmissionId = submissionId,
                SubmissionPeriod = "Test 1",
                Year = 2024
            },
            new SubmissionGetResponse
            {
                SubmissionId = submissionId,
                SubmissionPeriod = "Test 2",
                Year = 2024
            },
            new SubmissionGetResponse
            {
                SubmissionId = submissionId,
                SubmissionPeriod = "Test 3",
                Year = 2024
            },
        };

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, submissions.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetSubmissionsByFilter(organisationId, complianceSchemaId, year, submissionType);

        // Assert
        result.Should().BeEquivalentTo(submissions);
        result.Should().HaveCount(3);
    }

    [TestMethod]
    public async Task GetSubmissionByFilter_WithOutComplianceSchemaId_ReturnsExpectedValue()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var year = 2024;
        var submissionType = SubmissionType.Producer;

        var submissions = new List<SubmissionGetResponse>
        {
            new SubmissionGetResponse
            {
                SubmissionId = submissionId,
                SubmissionPeriod = "Test 1",
                Year = 2024
            },
            new SubmissionGetResponse
            {
                SubmissionId = submissionId,
                SubmissionPeriod = "Test 2",
                Year = 2024
            },
            new SubmissionGetResponse
            {
                SubmissionId = submissionId,
                SubmissionPeriod = "Test 3",
                Year = 2024
            },
        };

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, submissions.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetSubmissionsByFilter(organisationId, Guid.Empty, year, submissionType);

        // Assert
        result.Should().BeEquivalentTo(submissions);
        result.Should().HaveCount(3);
    }

    [TestMethod]
    public async Task GetSubmissionByFilter_WithoutYear_ReturnsExpectedValue()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var complianceSchemaId = Guid.NewGuid();
        var submissionType = SubmissionType.Producer;

        var submissions = new List<SubmissionGetResponse>
        {
            new SubmissionGetResponse
            {
                SubmissionId = submissionId,
                SubmissionPeriod = "Test 1",
                Year = 2024
            },
            new SubmissionGetResponse
            {
                SubmissionId = submissionId,
                SubmissionPeriod = "Test 2",
                Year = 2024
            },
            new SubmissionGetResponse
            {
                SubmissionId = submissionId,
                SubmissionPeriod = "Test 3",
                Year = 2024
            },
        };

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, submissions.ToJsonContent());

        // Act
        var result = await _systemUnderTest.GetSubmissionsByFilter(organisationId, complianceSchemaId, null, submissionType);

        // Assert
        result.Should().BeEquivalentTo(submissions);
        result.Should().HaveCount(3);
    }

    [TestMethod]
    public async Task GetRegistrationValidationErrorsRowsAsync_LogsAndThrowsException_WhenHttpClientResponseIsInternalServerError()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.GetRegistrationValidationErrorsAsync(submissionId))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error getting registration validation errors"));
    }
}
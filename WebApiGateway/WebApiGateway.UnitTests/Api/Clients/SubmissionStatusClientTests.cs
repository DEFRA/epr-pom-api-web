using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.SubmissionMicroservice.Application.Features.Queries.Common;
using EPR.SubmissionMicroservice.Data.Entities.SubmissionEvent;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
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
    private static readonly IFixture Fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly UserAccount _userAccount = Fixture.Create<UserAccount>();

    private Mock<ILogger<SubmissionStatusClient>> _loggerMock;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private Mock<IAccountServiceClient> _accountServiceClientMock;
    private SubmissionStatusClient _systemUnderTest;

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
        var antivirusCheckEvent = Fixture.Create<AntivirusCheckEvent>();
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
        var antivirusCheckEvent = Fixture.Create<AntivirusCheckEvent>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, new StringContent("Error Occured"));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.CreateEventAsync(antivirusCheckEvent, submissionId))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError("Error creating AntivirusCheck event, responseContent Error Occured"));
    }

    [TestMethod]
    public async Task CreateApplicationSubmittedEventAsync_DoesNotThrowException_WhenHttpClientResponseIsCreated()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var applicationSubmittedEvent = Fixture.Create<RegistrationApplicationSubmittedEvent>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.Created, null);

        // Act & Assert
        await _systemUnderTest.Invoking(x => x.CreateApplicationSubmittedEventAsync(applicationSubmittedEvent, submissionId)).Should().NotThrowAsync();

        var expectedRequestUri = new Uri($"https://example.com/submissions/{submissionId}/events");
        var expectedHeaders = new Dictionary<string, string>
        {
            { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
            { "UserId", _userAccount.User.Id.ToString() }
        };

        _httpMessageHandlerMock.VerifyRequest(HttpMethod.Post, expectedRequestUri, expectedHeaders, Times.Once());
    }

    [TestMethod]
    public async Task CreateApplicationSubmittedEventAsync_LogsAndThrowsException_WhenHttpClientResponseIsInternalServerError()
    {
        // Arrange
        var applicationSubmittedEvent = Fixture.Create<RegistrationApplicationSubmittedEvent>();

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, new StringContent("Error Occured"));

        // Act / Assert
        await _systemUnderTest.Invoking(x => x.CreateApplicationSubmittedEventAsync(applicationSubmittedEvent, Guid.NewGuid())).Should().ThrowAsync<HttpRequestException>();
        _loggerMock.VerifyLog(x => x.LogError("Error creating RegistrationApplicationSubmitted event, responseContent Error Occured"));
    }

    [TestMethod]
    public async Task CreateRegistrationFeePaymentEventAsync_DoesNotThrowException_WhenHttpClientResponseIsCreated()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var registrationFeePaymentEvent = Fixture.Create<RegistrationFeePaymentEvent>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.Created, null);

        // Act & Assert
        await _systemUnderTest.Invoking(x => x.CreateRegistrationFeePaymentEventAsync(registrationFeePaymentEvent, submissionId)).Should().NotThrowAsync();

        var expectedRequestUri = new Uri($"https://example.com/submissions/{submissionId}/events");
        var expectedHeaders = new Dictionary<string, string>
        {
            { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
            { "UserId", _userAccount.User.Id.ToString() }
        };

        _httpMessageHandlerMock.VerifyRequest(HttpMethod.Post, expectedRequestUri, expectedHeaders, Times.Once());
    }

    [TestMethod]
    public async Task CreateRegistrationFeePaymentEventAsync_LogsAndThrowsException_WhenHttpClientResponseIsInternalServerError()
    {
        // Arrange
        var registrationFeePaymentEvent = Fixture.Create<RegistrationFeePaymentEvent>();

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, new StringContent("Error Occured"));

        // Act / Assert
        await _systemUnderTest.Invoking(x => x.CreateRegistrationFeePaymentEventAsync(registrationFeePaymentEvent, Guid.NewGuid())).Should().ThrowAsync<HttpRequestException>();
        _loggerMock.VerifyLog(x => x.LogError("Error creating RegistrationFeePayment event, responseContent Error Occured"));
    }

    [TestMethod]
    public async Task CreateSubmissionAsync_DoesNotThrowException_WhenHttpClientResponseIsCreated()
    {
        // Arrange
        var createSubmission = Fixture.Create<CreateSubmission>();
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
        var createSubmission = Fixture.Create<CreateSubmission>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, new StringContent("Error Occured"));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.CreateSubmissionAsync(createSubmission))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError("Error creating submission, responseContent Error Occured"));
    }

    [TestMethod]
    public async Task GetSubmissionAsync_ReturnsHttpResponseMessage()
    {
        // Arrange
        var submission = Fixture.Create<RegistrationSubmission>();
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
        var submissions = Fixture.Create<List<RegistrationSubmission>>();
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
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, new StringContent("Error Occured"));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.GetSubmissionsAsync(string.Empty))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error getting submissions, responseContent Error Occured"));
    }

    [TestMethod]
    public async Task GetProducerValidationErrorRowsAsync_ReturnsSubmissions_WhenHttpClientResponseIsOk()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var validationErrorRows = Fixture.Build<ProducerValidationIssueRow>()
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
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, new StringContent("Error Occured"));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.GetProducerValidationErrorRowsAsync(submissionId))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error getting producer validation errors, responseContent Error Occured"));
    }

    [TestMethod]
    public async Task GetProducerValidationWarningRowsAsync_ReturnsSubmissions_WhenHttpClientResponseIsOk()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var validationWarningRows = Fixture.Build<ProducerValidationIssueRow>()
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
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, new StringContent("Error Occured"));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.GetProducerValidationWarningRowsAsync(submissionId))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error getting producer validation warnings, responseContent Error Occured"));
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
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, new StringContent("Error Occured"));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.SubmitAsync(submissionId, submissionPayload))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error submitting submission with id {submissionId} and file id {fileId}, responseContent {responseContent}", submissionId, fileId, "Error Occured"));
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
        var registrationValidationErrorRows = Fixture.Build<RegistrationValidationError>()
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
        var userOne = Fixture.Create<UserAccount>();
        var userTwo = Fixture.Create<UserAccount>();
        const string QueryString = "?key=value";
        var submissionHistoryEventsResponse = new SubmissionHistoryEventsResponse
        {
            SubmittedEvents = Fixture.Build<SubmittedEventResponse>()
            .With(x => x.SubmittedBy, "First Last")
            .CreateMany(2)
            .ToList(),
            RegulatorDecisionEvents = Fixture.Build<RegulatorDecisionEventResponse>()
            .CreateMany()
            .ToList(),
            AntivirusCheckEvents = Fixture.Build<AntivirusCheckEventResponse>()
            .CreateMany()
            .ToList()
        };

        _httpMessageHandlerMock.RespondWith(HttpStatusCode.OK, submissionHistoryEventsResponse.ToJsonContent());

        var userIdOne = submissionHistoryEventsResponse.SubmittedEvents[0].UserId;
        var userIdTwo = submissionHistoryEventsResponse.SubmittedEvents[1].UserId;

        _accountServiceClientMock.Setup(x => x.GetUserAccount(userIdOne)).ReturnsAsync(userOne);
        _accountServiceClientMock.Setup(x => x.GetUserAccount(userIdTwo)).ReturnsAsync(userTwo);

        // Act
        var result = await _systemUnderTest.GetSubmissionPeriodHistory(submissionId, QueryString);

        // Assert
        result.RegulatorDecisionEvents.Should().BeEquivalentTo(submissionHistoryEventsResponse.RegulatorDecisionEvents);
        result.AntivirusCheckEvents.Should().BeEquivalentTo(submissionHistoryEventsResponse.AntivirusCheckEvents);

        result.SubmittedEvents[0].SubmittedBy.Should().BeEquivalentTo(userOne.User.FirstName + " " + userOne.User.LastName);
        result.SubmittedEvents[1].SubmittedBy.Should().BeEquivalentTo(userTwo.User.FirstName + " " + userTwo.User.LastName);

        var expectedMethod = HttpMethod.Get;
        var expectedRequestUri = new Uri($"https://example.com/submissions/events/events-by-type/{submissionId}{QueryString}");
        var expectedHeaders = new Dictionary<string, string>
    {
        { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
        { "UserId", _userAccount.User.Id.ToString() }
    };

        _httpMessageHandlerMock.VerifyRequest(expectedMethod, expectedRequestUri, expectedHeaders, Times.Once());

        _accountServiceClientMock.Verify(x => x.GetUserAccount(userIdOne), Times.Once);
        _accountServiceClientMock.Verify(x => x.GetUserAccount(userIdTwo), Times.Once);
    }

    [TestMethod]
    public async Task GetSubmissionPeriodHistory_WhenSubmittedByIsNullOrMissing_CallsGetUserAccount()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var userOne = Fixture.Create<UserAccount>();
        var userTwo = Fixture.Create<UserAccount>();
        const string QueryString = "?key=value";
        var submissionHistoryEventsResponse = new SubmissionHistoryEventsResponse
        {
            SubmittedEvents = Fixture.Build<SubmittedEventResponse>()
            .Without(x => x.SubmittedBy)
            .CreateMany(2)
            .ToList(),
            RegulatorDecisionEvents = Fixture.Build<RegulatorDecisionEventResponse>()
            .CreateMany(2)
            .ToList(),
            AntivirusCheckEvents = Fixture.Build<AntivirusCheckEventResponse>()
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
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, new StringContent("Error Occured"));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.GetRegistrationValidationErrorsAsync(submissionId))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error getting registration validation errors, responseContent Error Occured"));
    }

    [TestMethod]
    public async Task GetRegistrationApplicationSubmissionDetails_Should_Return_Response_When_Successful()
    {
        // Arrange
        var queryString = "?id=123";
        var expectedResponse = new RegistrationApplicationDetails
        {
            SubmissionId = Guid.NewGuid(),
            IsSubmitted = true
        };
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(expectedResponse))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains(queryString)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _systemUnderTest.GetRegistrationApplicationDetails(queryString);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationSubmissionDetails_Should_Return_Null_When_NoContent()
    {
        // Arrange
        var queryString = "?id=123";
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NoContent
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains(queryString)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _systemUnderTest.GetRegistrationApplicationDetails(queryString);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetRegistrationApplicationSubmissionDetails_Should_Throw_Exception_On_HttpError()
    {
        // Arrange
        var queryString = "?id=123";
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains(queryString)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        Func<Task> act = async () => await _systemUnderTest.GetRegistrationApplicationDetails(queryString);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [TestMethod]
    public async Task GetRegistrationApplicationSubmissionDetails_Should_Throw_Exception_On_InvalidUrl()
    {
        // Arrange
        var queryString = "Https://localhost/test?id=123";
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains(queryString)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        Func<Task> act = async () => await _systemUnderTest.GetRegistrationApplicationDetails(queryString);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [TestMethod]
    public async Task CreateFileDownloadEventAsync_DoesNotThrowException_WhenHttpClientResponseIsCreated()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var fileDownloadCheckEvent = Fixture.Create<FileDownloadCheckEvent>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.Created, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.CreateFileDownloadEventAsync(fileDownloadCheckEvent, submissionId))
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
    public async Task CreateFileDownloadEventAsync_LogsAndThrowsException_WhenHttpClientResponseIsInternalServerError()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var fileDownloadCheckEvent = Fixture.Create<FileDownloadCheckEvent>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, new StringContent("Error Occured"));

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.CreateFileDownloadEventAsync(fileDownloadCheckEvent, submissionId))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError("Error creating FileDownloadCheck event, responseContent Error Occured"));
    }

    [TestMethod]
    public async Task GetFileScanResultAsync_ReturnsHttpResponseMessage()
    {
        // Arrange
        var submission = Fixture.Create<RegistrationSubmission>();
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
    public async Task GetFileScanResultAsync_ShouldReturnResponse_WhenSuccessful()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var expectedResponse = new AntivirusResultEvent
        {
            SubmissionId = Guid.NewGuid(),
            BlobName = Guid.NewGuid().ToString(),
            FileId = fileId,
            SubmissionType = SubmissionType.Registration,
            AntivirusScanResult = AntivirusScanResult.Success,
            AntivirusScanTrigger = AntivirusScanTrigger.Download,
            OrganisationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Errors = new List<string>()
        };

        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(expectedResponse))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains(fileId.ToString())),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _systemUnderTest.GetFileScanResultAsync(submissionId, fileId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [TestMethod]
    public async Task GetFileScanResultAsync_ShouldThrowException_OnHttpError()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains(fileId.ToString())),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        Func<Task> act = async () => await _systemUnderTest.GetFileScanResultAsync(submissionId, fileId);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [TestMethod]
    public async Task GetPackagingResubmissionApplicationDetails_Should_Return_Response_When_Successful()
    {
        // Arrange
        var queryString = "?id=123";
        var expectedResponse = new PackagingResubmissionApplicationDetails
        {
            SubmissionId = Guid.NewGuid(),
            IsSubmitted = true
        };
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(expectedResponse))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains(queryString)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _systemUnderTest.GetPackagingResubmissionApplicationDetails(queryString);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionApplicationDetailsSubmissionDetails_Should_Return_Null_When_NoContent()
    {
        // Arrange
        var queryString = "?id=123";
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NoContent
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains(queryString)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _systemUnderTest.GetPackagingResubmissionApplicationDetails(queryString);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetPackagingResubmissionApplicationDetailsSubmissionDetails_Should_Throw_Exception_On_HttpError()
    {
        // Arrange
        var queryString = "?id=123";
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains(queryString)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        Func<Task> act = async () => await _systemUnderTest.GetPackagingResubmissionApplicationDetails(queryString);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [TestMethod]
    public async Task GetPackagingResubmissionApplicationDetailsSubmissionDetails_Should_Throw_Exception_On_InvalidUrl()
    {
        // Arrange
        var queryString = "Https://localhost/test?id=123";
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains(queryString)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        Func<Task> act = async () => await _systemUnderTest.GetPackagingResubmissionApplicationDetails(queryString);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [TestMethod]
    public async Task CreateEventAsync_DoesNotThrowException_WhenHttpClientResponseIsCreated_And_PackagingResubmissionReferenceNumberIsCreated()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var packagingResubmissionReferenceNumberCreatedEvent = Fixture.Create<PackagingResubmissionReferenceNumberCreatedEvent>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.Created, null);

        // Act & Assert
        await _systemUnderTest.Invoking(x => x.CreateEventAsync(packagingResubmissionReferenceNumberCreatedEvent, submissionId)).Should().NotThrowAsync();

        var expectedRequestUri = new Uri($"https://example.com/submissions/{submissionId}/events");
        var expectedHeaders = new Dictionary<string, string>
        {
            { "OrganisationId", _userAccount.User.Organisations.First().Id.ToString() },
            { "UserId", _userAccount.User.Id.ToString() }
        };

        _httpMessageHandlerMock.VerifyRequest(HttpMethod.Post, expectedRequestUri, expectedHeaders, Times.Once());
    }

    [TestMethod]
    public async Task CreateEventAsync_LogsAndThrowsException_WhenHttpClientResponseIsInternalServerError_With_PackagingResubmissionReferenceNumberEvent()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var packagingResubmissionReferenceNumberCreatedEvent = Fixture.Create<PackagingResubmissionReferenceNumberCreatedEvent>();
        _httpMessageHandlerMock.RespondWith(HttpStatusCode.InternalServerError, null);

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.CreateEventAsync(packagingResubmissionReferenceNumberCreatedEvent, submissionId))
            .Should()
            .ThrowAsync<HttpRequestException>();

        _loggerMock.VerifyLog(x => x.LogError(It.IsAny<HttpRequestException>(), "Error creating PackagingResubmissionReferenceNumberCreated event"));
    }
}
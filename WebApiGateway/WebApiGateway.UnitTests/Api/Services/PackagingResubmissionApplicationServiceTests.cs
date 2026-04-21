using EPR.SubmissionMicroservice.Application.Features.Queries.Common;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.PackagingResubmissionApplication;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class PackagingResubmissionApplicationServiceTests
{
    private Mock<ISubmissionStatusClient> _submissionStatusClientMock;
    private PackagingResubmissionApplicationService _service;
    private Mock<ICommondataClient> _commondataClientMock;

    [TestInitialize]
    public void SetUp()
    {
        _submissionStatusClientMock = new Mock<ISubmissionStatusClient>();
        _commondataClientMock = new Mock<ICommondataClient>();
        _service = new PackagingResubmissionApplicationService(_submissionStatusClientMock.Object, _commondataClientMock.Object);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionApplicationDetails_ShouldReturnNull_WhenSubmissionStatusClientReturnsNull()
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetPackagingResubmissionApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync((List<PackagingResubmissionApplicationDetails>)null);

        // Act
        var result = await _service.GetPackagingResubmissionApplicationDetails("test");

        // Assert
        result.Should().BeNull();
        _submissionStatusClientMock.Verify(client => client.GetPackagingResubmissionApplicationDetails(It.IsAny<string>()), Times.Once);
    }

    [DataTestMethod]
    [DataRow(false, "", true)]
    [DataRow(true, "", true)]
    [DataRow(false, " ", false)]
    [DataRow(true, " ", false)]
    [DataRow(false, null, true)]
    [DataRow(true, null, true)]
    [DataRow(false, "Error", false)]
    [DataRow(true, "Error", false)]
    public async Task GetPackagingResubmissionApplicationDetails_ShouldReturnExpectedSyncStatusResults_WhenSubmissionStatusClientReturnsAValidResponse(bool fileSyncStatus, string errorMessage, bool syncComplete)
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetPackagingResubmissionApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync([
                new PackagingResubmissionApplicationDetails
                {
                    IsSubmitted = true,
                    LastSubmittedFile = new PackagingResubmissionApplicationDetails.LastSubmittedFileDetails
                        { FileId = Guid.NewGuid() },
                    SubmissionId = Guid.NewGuid()
                }
            ]);

        var testResponse = new PackagingResubmissionMemberResponse { MemberCount = 1, ErrorMessage = errorMessage, ReferenceNumber = "124356" };
        _commondataClientMock
            .Setup(x => x.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), null))
            .ReturnsAsync(testResponse);

        _commondataClientMock.Setup(x => x.GetPackagingResubmissionFileSyncStatusFromSynapse(It.IsAny<Guid>())).ReturnsAsync(fileSyncStatus);

        // Act
        var result = await _service.GetPackagingResubmissionApplicationDetails("test");

        // Assert
        result.Should().NotBeNull();
        result[0].Should().NotBeNull();
        result[0]?.SynapseResponse.Should().NotBeNull();
        result[0]?.SynapseResponse.IsFileSynced.Should().Be(fileSyncStatus);
        result[0]?.SynapseResponse.IsResubmissionDataSynced.Should().Be(syncComplete);
        _commondataClientMock.Verify(client => client.GetPackagingResubmissionFileSyncStatusFromSynapse(It.IsAny<Guid>()), Times.Once);
        _commondataClientMock.Verify(client => client.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), null), Times.Once);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionApplicationDetails_ShouldThrowException_WhenSubmissionStatusClientThrowsException()
    {
        // Arrange
        var expectedException = new HttpRequestException("An error occurred while fetching data.");

        _submissionStatusClientMock
            .Setup(x => x.GetPackagingResubmissionApplicationDetails(It.IsAny<string>()))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () => await _service.GetPackagingResubmissionApplicationDetails("Test");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage(expectedException.Message);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionMemberDetails_ShouldReturnNull_WhenFeeCalculationDetailsClientReturnsNull()
    {
        // Arrange
        _commondataClientMock.Setup(x => x.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync((PackagingResubmissionMemberResponse)null);

        // Act
        var result = await _service.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>());

        // Assert
        result.Should().BeNull();
        _commondataClientMock.Verify(client => client.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionMemberDetails_ShouldThrowException_WhenFeeCalculationDetailsClientThrowsException()
    {
        // Arrange
        var expectedException = new HttpRequestException("An error occurred while fetching data.");

        _commondataClientMock
            .Setup(x => x.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>()))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () => await _service.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>());

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage(expectedException.Message);
    }

    [TestMethod]
    public async Task CreateEventAsync_ValidEvent_CallsSubmissionStatusClient()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var @event = new Mock<AbstractEvent>().Object;

        _submissionStatusClientMock
            .Setup(client => client.CreateEventAsync(@event, submissionId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateEventAsync(@event, submissionId);

        // Assert
        _submissionStatusClientMock.Verify(client => client.CreateEventAsync(@event, submissionId), Times.Once);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionApplicationDetails_ShouldNotAssessSynapseSyncStatuses_WhenLastSubmittedFileIdIsNull()
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetPackagingResubmissionApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync([
                new PackagingResubmissionApplicationDetails
                {
                    IsSubmitted = true,
                    LastSubmittedFile = new PackagingResubmissionApplicationDetails.LastSubmittedFileDetails
                        { FileId = null }
                }
            ]);

        _commondataClientMock.Setup(x => x.GetPackagingResubmissionFileSyncStatusFromSynapse(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await _service.GetPackagingResubmissionApplicationDetails("test");

        // Assert
        result.Should().NotBeNull();
        _commondataClientMock.Verify(client => client.GetPackagingResubmissionFileSyncStatusFromSynapse(It.IsAny<Guid>()), Times.Never);
        _commondataClientMock.Verify(client => client.GetPackagingResubmissionSyncStatusFromSynapse(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionApplicationDetails_ShouldNotAssessSynapseSyncStatuses_WhenIsSubmittedIsFalse()
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetPackagingResubmissionApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync([
                new PackagingResubmissionApplicationDetails
                {
                    IsSubmitted = false,
                    LastSubmittedFile = new PackagingResubmissionApplicationDetails.LastSubmittedFileDetails
                        { FileId = Guid.NewGuid() }
                }
            ]);

        _commondataClientMock.Setup(x => x.GetPackagingResubmissionFileSyncStatusFromSynapse(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await _service.GetPackagingResubmissionApplicationDetails("test");

        // Assert
        result.Should().NotBeNull();
        _commondataClientMock.Verify(client => client.GetPackagingResubmissionFileSyncStatusFromSynapse(It.IsAny<Guid>()), Times.Never);
        _commondataClientMock.Verify(client => client.GetPackagingResubmissionSyncStatusFromSynapse(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task GetActualSubmissionPeriod_ShouldReturnCorrectObject()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        const string SubmissionPeriod = "July to December 2025";
        const string ActualSubmissionPeriod = "January to December 2025";

        var response = new PackagingResubmissionActualSubmissionPeriodResponse()
        {
            ActualSubmissionPeriod = ActualSubmissionPeriod,
        };

        _commondataClientMock
            .Setup(x => x.GetActualSubmissionPeriod(submissionId, SubmissionPeriod))
            .ReturnsAsync(response);

        // Act
        var result = await _service.GetActualSubmissionPeriod(submissionId, SubmissionPeriod);

        // Assert
        result!.Should().BeEquivalentTo(response);
        result.ActualSubmissionPeriod.Should().BeEquivalentTo(ActualSubmissionPeriod);

        _commondataClientMock.Verify(x => x.GetActualSubmissionPeriod(submissionId, SubmissionPeriod), Times.Once);
    }
}
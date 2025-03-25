using EPR.SubmissionMicroservice.Application.Features.Queries.Common;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.PackagingResubmissionApplication;

namespace WebApiGateway.Api.Services;

[TestClass]
public class PackagingResubmissionApplicationServiceTests
{
    private readonly Guid _fileId = Guid.NewGuid();
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
            .ReturnsAsync((PackagingResubmissionApplicationDetails)null);

        // Act
        var result = await _service.GetPackagingResubmissionApplicationDetails("test");

        // Assert
        result.Should().BeNull();
        _submissionStatusClientMock.Verify(client => client.GetPackagingResubmissionApplicationDetails(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionApplicationDetails_ShouldReturnResult_WhenSubmissionStatusClientReturnsNotNull()
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetPackagingResubmissionApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new PackagingResubmissionApplicationDetails { IsSubmitted = true, LastSubmittedFile = new PackagingResubmissionApplicationDetails.LastSubmittedFileDetails { FileId = new Guid() } });

        _commondataClientMock.Setup(x => x.GetPackagingResubmissionFileDetailsFromSynapse(It.IsAny<Guid>())).ReturnsAsync(new Core.Models.Commondata.SynapseResponse());

        // Act
        var result = await _service.GetPackagingResubmissionApplicationDetails("test");

        // Assert
        result.Should().NotBeNull();
        _commondataClientMock.Verify(client => client.GetPackagingResubmissionFileDetailsFromSynapse(It.IsAny<Guid>()), Times.Once);
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
}
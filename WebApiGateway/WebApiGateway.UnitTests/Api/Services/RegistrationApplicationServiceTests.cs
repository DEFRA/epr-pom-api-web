using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Models.Submission;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class RegistrationApplicationServiceTests
{
    private readonly Guid _fileId = Guid.NewGuid();
    private Mock<IRegistrationFeeCalculationDetailsClient> _feeCalculationDetailsClientMock;
    private Mock<ISubmissionStatusClient> _submissionStatusClientMock;
    private RegistrationApplicationService _service;

    [TestInitialize]
    public void SetUp()
    {
        _feeCalculationDetailsClientMock = new Mock<IRegistrationFeeCalculationDetailsClient>();
        _submissionStatusClientMock = new Mock<ISubmissionStatusClient>();
        _service = new RegistrationApplicationService(_submissionStatusClientMock.Object, _feeCalculationDetailsClientMock.Object);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldCallFeeCalculationDetailsClientWithCorrectFileId()
    {
        // Arrange
        _feeCalculationDetailsClientMock
            .Setup(client => client.GetRegistrationFeeCalculationDetails(_fileId, null, null))
            .ReturnsAsync([]);

        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                IsSubmitted = true,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails
                {
                    FileId = _fileId
                }
            });

        // Act
        var result = await _service.GetRegistrationApplicationDetails("test");

        // Assert
        result.Should().NotBeNull();
        _submissionStatusClientMock.Verify(client => client.GetRegistrationApplicationDetails(It.IsAny<string>()), Times.Once);
        _feeCalculationDetailsClientMock.Verify(client => client.GetRegistrationFeeCalculationDetails(_fileId, null, null), Times.Once);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldNotCallFeeCalculationDetailsClientWhenLastSubmittedFileIsNull()
    {
        // Arrange
        _feeCalculationDetailsClientMock
            .Setup(client => client.GetRegistrationFeeCalculationDetails(_fileId, null, null))
            .ReturnsAsync([]);

        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                IsSubmitted = true,
                LastSubmittedFile = null
            });

        // Act
        var result = await _service.GetRegistrationApplicationDetails("test");

        // Assert
        result.Should().NotBeNull();
        _submissionStatusClientMock.Verify(client => client.GetRegistrationApplicationDetails(It.IsAny<string>()), Times.Once);
        _feeCalculationDetailsClientMock.Verify(client => client.GetRegistrationFeeCalculationDetails(_fileId, null, null), Times.Never);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldNotCallFeeCalculationDetailsClientWhenFileIdIsNull()
    {
        // Arrange
        _feeCalculationDetailsClientMock
            .Setup(client => client.GetRegistrationFeeCalculationDetails(_fileId, null, null))
            .ReturnsAsync([]);

        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                IsSubmitted = true,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails
                {
                    FileId = null
                }
            });

        // Act
        var result = await _service.GetRegistrationApplicationDetails("test");

        // Assert
        result.Should().NotBeNull();
        _submissionStatusClientMock.Verify(client => client.GetRegistrationApplicationDetails(It.IsAny<string>()), Times.Once);
        _feeCalculationDetailsClientMock.Verify(client => client.GetRegistrationFeeCalculationDetails(_fileId, null, null), Times.Never);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldNotCallFeeCalculationDetailsClientWhenIsSubmittedIsFalse()
    {
        // Arrange
        _feeCalculationDetailsClientMock
            .Setup(client => client.GetRegistrationFeeCalculationDetails(_fileId, null, null))
            .ReturnsAsync([]);

        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                IsSubmitted = false,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails
                {
                    FileId = _fileId
                }
            });

        // Act
        var result = await _service.GetRegistrationApplicationDetails("test");

        // Assert
        result.Should().NotBeNull();
        _submissionStatusClientMock.Verify(client => client.GetRegistrationApplicationDetails(It.IsAny<string>()), Times.Once);
        _feeCalculationDetailsClientMock.Verify(client => client.GetRegistrationFeeCalculationDetails(_fileId, null, null), Times.Never);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldReturnNull_WhenSubmissionStatusClientReturnsNull()
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync((RegistrationApplicationDetails)null);

        // Act
        var result = await _service.GetRegistrationApplicationDetails("test");

        // Assert
        result.Should().BeNull();
        _submissionStatusClientMock.Verify(client => client.GetRegistrationApplicationDetails(It.IsAny<string>()), Times.Once);
        _feeCalculationDetailsClientMock.Verify(client => client.GetRegistrationFeeCalculationDetails(_fileId, null, null), Times.Never);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldThrowException_WhenSubmissionStatusClientThrowsException()
    {
        // Arrange
        var expectedException = new HttpRequestException("An error occurred while fetching data.");

        _submissionStatusClientMock
            .Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () => await _service.GetRegistrationApplicationDetails("Test");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage(expectedException.Message);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldThrowException_WhenFeeCalculationDetailsClientThrowsException()
    {
        // Arrange
        var expectedException = new HttpRequestException("An error occurred while fetching data.");

        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                IsSubmitted = true,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails
                {
                    FileId = _fileId
                }
            });

        _feeCalculationDetailsClientMock
            .Setup(x => x.GetRegistrationFeeCalculationDetails(_fileId, null, null))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () => await _service.GetRegistrationApplicationDetails("Test");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage(expectedException.Message);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldCallFeeCalculationDetailsClientWithCorrectFileIdAndDeadLineDates()
    {
        // Arrange
        _feeCalculationDetailsClientMock
            .Setup(client => client.GetRegistrationFeeCalculationDetails(_fileId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync([]);

        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                IsSubmitted = true,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails
                {
                    FileId = _fileId
                }
            });

        // Act
        var result = await _service.GetRegistrationApplicationDetails("?OrganisationNumber=131387&OrganisationId=13b5f39c-641a-4916-9040-207ed2b70ccd&SubmissionPeriod=January to December 2026&SmallProducerLateFeeDeadLine=2026-04-01T00:00:00.0000000&LargeProducerLateFeeDeadLine=2025-10-01T00:00:00.0000000");

        // Assert
        result.Should().NotBeNull();
        _submissionStatusClientMock.Verify(client => client.GetRegistrationApplicationDetails(It.IsAny<string>()), Times.Once);
        _feeCalculationDetailsClientMock.Verify(client => client.GetRegistrationFeeCalculationDetails(_fileId, new DateTime(2025, 10, 01, 0, 0, 0), new DateTime(2026, 04, 01, 0, 0, 0)), Times.Once);
    }
}
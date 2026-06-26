using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Models.RegistrationFeeCalculation;
using WebApiGateway.Core.Models.Submission;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class RegistrationApplicationServiceTests
{
    private readonly Guid _fileId = Guid.NewGuid();
    private readonly Guid _submissionId = Guid.NewGuid();
    private Mock<IRegistrationFeeCalculationDetailsClient> _feeCalculationDetailsClientMock;
    private Mock<ISubmissionStatusClient> _submissionStatusClientMock;
    private Mock<IPaymentServiceClient> _paymentServiceClientMock;
    private RegistrationApplicationService _service;

    [TestInitialize]
    public void SetUp()
    {
        _feeCalculationDetailsClientMock = new Mock<IRegistrationFeeCalculationDetailsClient>();
        _submissionStatusClientMock = new Mock<ISubmissionStatusClient>();
        _paymentServiceClientMock = new Mock<IPaymentServiceClient>();
        _service = new RegistrationApplicationService(
            _submissionStatusClientMock.Object,
            _feeCalculationDetailsClientMock.Object,
            _paymentServiceClientMock.Object);
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
        _submissionStatusClientMock.Verify(c => c.GetRegistrationApplicationDetails(It.IsAny<string>()), Times.Once);
        _paymentServiceClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(It.IsAny<Guid>()), Times.Never);
        _feeCalculationDetailsClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldNotCallFeeClientsWhenLastSubmittedFileIsNull()
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                SubmissionId = _submissionId,
                IsSubmitted = true,
                LastSubmittedFile = null
            });

        // Act
        var result = await _service.GetRegistrationApplicationDetails("test");

        // Assert
        result.Should().NotBeNull();
        _paymentServiceClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(It.IsAny<Guid>()), Times.Never);
        _feeCalculationDetailsClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldNotCallFeeClientsWhenFileIdIsNull()
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                SubmissionId = _submissionId,
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
        _paymentServiceClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(It.IsAny<Guid>()), Times.Never);
        _feeCalculationDetailsClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldNotCallFeeClientsWhenIsSubmittedIsFalse()
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                SubmissionId = _submissionId,
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
        _paymentServiceClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(It.IsAny<Guid>()), Times.Never);
        _feeCalculationDetailsClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldNotCallFeeClientsWhenSubmissionIdIsNull()
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                SubmissionId = null,
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
        _paymentServiceClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(It.IsAny<Guid>()), Times.Never);
        _feeCalculationDetailsClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldCallPaymentServiceWithSubmissionId()
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                SubmissionId = _submissionId,
                IsSubmitted = true,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails { FileId = _fileId }
            });

        _paymentServiceClientMock
            .Setup(c => c.GetRegistrationFeeCalculationDetails(_submissionId))
            .ReturnsAsync([]);

        // Act
        var result = await _service.GetRegistrationApplicationDetails("test");

        // Assert
        result.Should().NotBeNull();
        _paymentServiceClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(_submissionId), Times.Once);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldUsePaymentServiceResult_WhenItReturnsData()
    {
        // Arrange
        var paymentFeeDetails = new RegistrationFeeCalculationDetails { OrganisationSize = "Large" };

        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                SubmissionId = _submissionId,
                IsSubmitted = true,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails { FileId = _fileId }
            });

        _paymentServiceClientMock
            .Setup(c => c.GetRegistrationFeeCalculationDetails(_submissionId))
            .ReturnsAsync([paymentFeeDetails]);

        // Act
        var result = await _service.GetRegistrationApplicationDetails("test");

        // Assert
        result!.RegistrationFeeCalculationDetails.Should().ContainSingle()
            .Which.OrganisationSize.Should().Be("Large");
        _feeCalculationDetailsClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldNotCallLegacyClient_WhenPaymentServiceReturnsData()
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                SubmissionId = _submissionId,
                IsSubmitted = true,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails { FileId = _fileId }
            });

        _paymentServiceClientMock
            .Setup(c => c.GetRegistrationFeeCalculationDetails(_submissionId))
            .ReturnsAsync([new RegistrationFeeCalculationDetails()]);

        // Act
        await _service.GetRegistrationApplicationDetails("test");

        // Assert
        _feeCalculationDetailsClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldFallBackToLegacyClient_WhenPaymentServiceReturnsNull()
    {
        // Arrange
        var legacyFeeDetails = new RegistrationFeeCalculationDetails { OrganisationSize = "Small" };

        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                SubmissionId = _submissionId,
                IsSubmitted = true,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails { FileId = _fileId }
            });

        _paymentServiceClientMock
            .Setup(c => c.GetRegistrationFeeCalculationDetails(_submissionId))
            .ReturnsAsync((RegistrationFeeCalculationDetails[]?)null);

        _feeCalculationDetailsClientMock
            .Setup(c => c.GetRegistrationFeeCalculationDetails(_fileId))
            .ReturnsAsync([legacyFeeDetails]);

        // Act
        var result = await _service.GetRegistrationApplicationDetails("test");

        // Assert
        result!.RegistrationFeeCalculationDetails.Should().ContainSingle()
            .Which.OrganisationSize.Should().Be("Small");
        _feeCalculationDetailsClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(_fileId), Times.Once);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldCallLegacyClientWithFileId_WhenPaymentServiceReturnsNull()
    {
        // Arrange
        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                SubmissionId = _submissionId,
                IsSubmitted = true,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails { FileId = _fileId }
            });

        _paymentServiceClientMock
            .Setup(c => c.GetRegistrationFeeCalculationDetails(_submissionId))
            .ReturnsAsync((RegistrationFeeCalculationDetails[]?)null);

        _feeCalculationDetailsClientMock
            .Setup(c => c.GetRegistrationFeeCalculationDetails(_fileId))
            .ReturnsAsync([]);

        // Act
        await _service.GetRegistrationApplicationDetails("test");

        // Assert
        _feeCalculationDetailsClientMock.Verify(c => c.GetRegistrationFeeCalculationDetails(_fileId), Times.Once);
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
    public async Task GetRegistrationApplicationDetails_ShouldThrowException_WhenPaymentServiceClientThrowsException()
    {
        // Arrange
        var expectedException = new HttpRequestException("An error occurred while fetching data.");

        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                SubmissionId = _submissionId,
                IsSubmitted = true,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails { FileId = _fileId }
            });

        _paymentServiceClientMock
            .Setup(c => c.GetRegistrationFeeCalculationDetails(_submissionId))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () => await _service.GetRegistrationApplicationDetails("test");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage(expectedException.Message);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ShouldThrowException_WhenLegacyFeeClientThrowsException()
    {
        // Arrange
        var expectedException = new HttpRequestException("An error occurred while fetching data.");

        _submissionStatusClientMock.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                SubmissionId = _submissionId,
                IsSubmitted = true,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails { FileId = _fileId }
            });

        _paymentServiceClientMock
            .Setup(c => c.GetRegistrationFeeCalculationDetails(_submissionId))
            .ReturnsAsync((RegistrationFeeCalculationDetails[]?)null);

        _feeCalculationDetailsClientMock
            .Setup(c => c.GetRegistrationFeeCalculationDetails(_fileId))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () => await _service.GetRegistrationApplicationDetails("test");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage(expectedException.Message);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_PassesThroughClosedLoopRecyclingFields()
    {
        // Arrange
        var feeDetails = new RegistrationFeeCalculationDetails
        {
            IsClosedLoopRecycling = true,
            NumberOfSubsidiariesBeingClosedLoopRecycling = 4
        };

        _submissionStatusClientMock
            .Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()))
            .ReturnsAsync(new RegistrationApplicationDetails
            {
                SubmissionId = _submissionId,
                IsSubmitted = true,
                LastSubmittedFile = new RegistrationApplicationDetails.LastSubmittedFileDetails { FileId = _fileId }
            });

        _paymentServiceClientMock
            .Setup(c => c.GetRegistrationFeeCalculationDetails(_submissionId))
            .ReturnsAsync([feeDetails]);

        // Act
        var result = await _service.GetRegistrationApplicationDetails("test");

        // Assert
        result!.RegistrationFeeCalculationDetails.Should().ContainSingle();
        result.RegistrationFeeCalculationDetails![0].IsClosedLoopRecycling.Should().BeTrue();
        result.RegistrationFeeCalculationDetails[0].NumberOfSubsidiariesBeingClosedLoopRecycling.Should().Be(4);
    }
}

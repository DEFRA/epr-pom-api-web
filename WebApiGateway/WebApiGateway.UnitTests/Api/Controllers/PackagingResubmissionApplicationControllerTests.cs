using EPR.SubmissionMicroservice.Application.Features.Queries.Common;
using EPR.SubmissionMicroservice.Data.Entities.SubmissionEvent;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Controllers;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Events;
using WebApiGateway.Core.Models.PackagingResubmissionApplication;

namespace WebApiGateway.UnitTests.Api.Controllers;

[TestClass]
public class PackagingResubmissionApplicationControllerTests
{
    private Mock<IPackagingResubmissionApplicationService> _packagingResubmissionApplicationService;
    private PackagingResubmissionApplicationController _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _packagingResubmissionApplicationService = new Mock<IPackagingResubmissionApplicationService>();
        _systemUnderTest = new PackagingResubmissionApplicationController(_packagingResubmissionApplicationService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
    }

    [TestMethod]
    public async Task GetPackagingResubmissionApplicationDetails_ReturnsOkObjectResult()
    {
        // Arrange
        var response = new List<PackagingResubmissionApplicationDetails>();

        _packagingResubmissionApplicationService.Setup(x => x.GetPackagingResubmissionApplicationDetails(It.IsAny<string>())).ReturnsAsync(response);

        // Act
        var result = await _systemUnderTest.GetPackagingResubmissionApplicationDetails() as OkObjectResult;

        // Assert
        result!.Value.Should().BeEquivalentTo(response);
        _packagingResubmissionApplicationService.Verify(x => x.GetPackagingResubmissionApplicationDetails(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionApplicationDetails_ReturnsNoContentResult()
    {
        // Arrange

        // Act
        var result = await _systemUnderTest.GetPackagingResubmissionApplicationDetails() as OkObjectResult;

        // Assert
        result!.Should().Be(null);

        _packagingResubmissionApplicationService.Verify(x => x.GetPackagingResubmissionApplicationDetails(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionApplicationDetails_When_GetComplianceSchemeDetails_is_Empty_ReturnsOkObjectResult()
    {
        // Arrange

        // Act
        var result = await _systemUnderTest.GetPackagingResubmissionApplicationDetails() as OkObjectResult;

        // Assert
        result.Should().Be(null);

        _packagingResubmissionApplicationService.Verify(x => x.GetPackagingResubmissionApplicationDetails(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionMemberDetails_ReturnsOkObjectResult()
    {
        // Arrange
        var response = new PackagingResubmissionMemberResponse();

        _packagingResubmissionApplicationService.Setup(x => x.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(response);

        // Act
        var result = await _systemUnderTest.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>()) as OkObjectResult;

        // Assert
        result!.Value.Should().BeEquivalentTo(response);

        _packagingResubmissionApplicationService.Verify(x => x.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionMemberDetails_ReturnsNoContentResult()
    {
        // Arrange

        // Act
        var result = await _systemUnderTest.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>()) as OkObjectResult;

        // Assert
        result!.Should().Be(null);

        _packagingResubmissionApplicationService.Verify(x => x.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task GetPackagingResubmissionMemberDetails_ReturnsResult_WithError()
    {
        // Arrange
        var response = new PackagingResubmissionMemberResponse { ErrorMessage = "Precondition failed error message." };

        _packagingResubmissionApplicationService.Setup(x => x.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(response);

        // Act
        var result = await _systemUnderTest.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>()) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status428PreconditionRequired);
        result.Value.Should().Be("Precondition failed error message.");

        _packagingResubmissionApplicationService.Verify(x => x.GetPackagingResubmissionMemberDetails(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task CreatePackagingResubmissionReferenceNumberEvent_ShouldCallCreateEventMethodOnce()
    {
        // Arrange
        var @event = new PackagingResubmissionReferenceNumberCreatedEvent()
        {
            PackagingResubmissionReferenceNumber = "test ref"
        };

        _packagingResubmissionApplicationService.Setup(x => x.CreateEventAsync(@event, It.IsAny<Guid>()));

        // Act
        var result = await _systemUnderTest.CreatePackagingResubmissionReferenceNumberEvent(It.IsAny<Guid>(), @event) as OkObjectResult;

        // Assert
        _packagingResubmissionApplicationService.Verify(
            x => x.CreateEventAsync(
                It.Is<PackagingResubmissionReferenceNumberCreatedEvent>(m =>
                m.PackagingResubmissionReferenceNumber == "test ref"),
                It.IsAny<Guid>()),
            Times.Once);
    }

    [TestMethod]
    public async Task CreatePackagingResubmissionFeeViewEvent_ShouldCallCreateEventMethodOnce()
    {
        // Arrange
        var @event = new PackagingResubmissionFeeViewCreatedEvent()
        {
            IsPackagingResubmissionFeeViewed = true,
            FileId = Guid.NewGuid()
        };

        _packagingResubmissionApplicationService.Setup(x => x.CreateEventAsync(@event, It.IsAny<Guid>()));

        // Act
        var result = await _systemUnderTest.CreatePackagingResubmissionFeeViewEvent(It.IsAny<Guid>(), @event) as OkObjectResult;

        // Assert
        _packagingResubmissionApplicationService.Verify(
            x => x.CreateEventAsync(
                It.Is<PackagingResubmissionFeeViewCreatedEvent>(m =>
                m.IsPackagingResubmissionFeeViewed == true),
                It.IsAny<Guid>()),
            Times.Once);
    }

    [TestMethod]
    public async Task CreatePackagingResubmissionPaymentMethodEvent_ShouldCallCreateEventMethodOnce()
    {
        // Arrange
        var @event = new PackagingDataResubmissionFeePaymentEvent()
        {
            PaymentMethod = "test payment method"
        };

        _packagingResubmissionApplicationService.Setup(x => x.CreateEventAsync(@event, It.IsAny<Guid>()));

        // Act
        var result = await _systemUnderTest.CreatePackagingResubmissionPaymentMethodEvent(It.IsAny<Guid>(), @event) as OkObjectResult;

        // Assert
        _packagingResubmissionApplicationService.Verify(
            x => x.CreateEventAsync(
                It.Is<PackagingDataResubmissionFeePaymentEvent>(m =>
                m.PaymentMethod == "test payment method"),
                It.IsAny<Guid>()),
            Times.Once);
    }

    [TestMethod]
    public async Task CreatePackagingResubmissionSubmittedEvent_ShouldCallCreateEventMethodOnce()
    {
        // Arrange
        var @event = new PackagingResubmissionApplicationSubmittedCreatedEvent()
        {
            IsResubmitted = true,
            SubmittedBy = "test user",
            SubmissionDate = DateTime.UtcNow
        };

        _packagingResubmissionApplicationService.Setup(x => x.CreateEventAsync(@event, It.IsAny<Guid>()));

        // Act
        var result = await _systemUnderTest.CreatePackagingResubmissionSubmittedEvent(It.IsAny<Guid>(), @event) as OkObjectResult;

        // Assert
        _packagingResubmissionApplicationService.Verify(
            x => x.CreateEventAsync(
                It.Is<PackagingResubmissionApplicationSubmittedCreatedEvent>(m =>
                m.IsResubmitted == true),
                It.IsAny<Guid>()),
            Times.Once);
    }
}
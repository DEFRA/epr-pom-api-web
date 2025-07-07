using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Controllers;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.UnitTests.Api.Controllers;

[TestClass]
public class FileUploadAccreditationControllerTests
{
    private const string Filename = "filename.csv";
    private readonly Guid _submissionId = Guid.NewGuid();
    private Mock<IFileUploadService> _mockFileUploadService;
    private FileUploadAccreditationController _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockFileUploadService = new Mock<IFileUploadService>();
        _systemUnderTest = new FileUploadAccreditationController(_mockFileUploadService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
    }

    [DataTestMethod]
    public async Task FileUpload_ReturnsBadRequestResult_WhenFilenameHeaderIsMissing()
    {
        // Arrange / Act
        var result = await _systemUnderTest.FileUploadAccreditation(string.Empty, SubmissionType.Accreditation, null) as BadRequestObjectResult;

        // Assert
        result.Value.As<ValidationProblemDetails>().Errors
            .Should()
            .HaveCount(1)
            .And
            .ContainKey("fileName")
            .WhoseValue
            .Should()
            .BeEquivalentTo("fileName header is required");

        _mockFileUploadService.Verify(
            x => x.UploadFileAccreditationAsync(
                It.IsAny<Stream>(),
                It.IsAny<SubmissionType>(),
                It.IsAny<string>(),
                null),
            Times.Never);
    }

    [DataTestMethod]
    public async Task FileUpload_ReturnsBadRequestResult_WhenSubmissionTypeIsWrong()
    {
        // Arrange / Act
        var result = await _systemUnderTest.FileUploadAccreditation(Filename, SubmissionType.Producer, null) as BadRequestObjectResult;

        // Assert
        result.Value.As<ValidationProblemDetails>().Errors
            .Should()
            .HaveCount(1)
            .And
            .ContainKey("submissionType")
            .WhoseValue
            .Should()
            .BeEquivalentTo("submissionType header must be accreditation");

        _mockFileUploadService.Verify(
            x => x.UploadFileAccreditationAsync(
                It.IsAny<Stream>(),
                It.IsAny<SubmissionType>(),
                It.IsAny<string>(),
                null),
            Times.Never);
    }

    [TestMethod]
    public async Task FileUploadAccreditation_ReturnsOkResult_WhenAllRequiredFieldsAreProvided()
    {
        // Arrange
        var filestream = new MemoryStream();
        _systemUnderTest.HttpContext.Request.Body = filestream;

        _mockFileUploadService.Setup(x => x.UploadFileAccreditationAsync(
            It.IsAny<Stream>(),
            It.IsAny<SubmissionType>(),
            It.IsAny<string>(),
            null))
            .ReturnsAsync(_submissionId);

        // Act
        var result = await _systemUnderTest.FileUploadAccreditation(Filename, SubmissionType.Accreditation, null) as CreatedAtRouteResult;

        // Assert
        result.RouteName.Should().Be(nameof(SubmissionController.GetSubmission));

        result.RouteValues.Should()
            .HaveCount(1)
            .And
            .ContainKey("submissionId")
            .WhoseValue
            .Should()
            .Be(_submissionId);

        _mockFileUploadService.Verify(
            x => x.UploadFileAccreditationAsync(
                filestream,
                SubmissionType.Accreditation,
                Filename,
                null),
            Times.Once);
    }

    [TestMethod]
    public async Task FileUploadAccreditation_ReturnsOkResult_WhenAllComplianceSchemeIdIsValid()
    {
        // Arrange
        var filestream = new MemoryStream();
        _systemUnderTest.HttpContext.Request.Body = filestream;
        var complianceSchemeId = Guid.NewGuid();

        _mockFileUploadService.Setup(x => x.UploadFileAccreditationAsync(
            It.IsAny<Stream>(),
            It.IsAny<SubmissionType>(),
            It.IsAny<string>(),
            complianceSchemeId))
            .ReturnsAsync(_submissionId);

        // Act
        var result = await _systemUnderTest.FileUploadAccreditation(Filename, SubmissionType.Accreditation, complianceSchemeId) as CreatedAtRouteResult;

        // Assert
        result.RouteName.Should().Be(nameof(SubmissionController.GetSubmission));

        result.RouteValues.Should()
            .HaveCount(1)
            .And
            .ContainKey("submissionId")
            .WhoseValue
            .Should()
            .Be(_submissionId);

        _mockFileUploadService.Verify(
            x => x.UploadFileAccreditationAsync(
                filestream,
                SubmissionType.Accreditation,
                Filename,
                complianceSchemeId),
            Times.Once);
    }
}

namespace WebApiGateway.UnitTests.Api.Controllers;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Controllers;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Enumeration;

[TestClass]
public class FileUploadSubsidiaryControllerTests
{
    private const string Filename = "filename.csv";
    private readonly Guid _submissionId = Guid.NewGuid();
    private Mock<IFileUploadService> _fileUploadServiceMock;
    private FileUploadSubsidiaryController _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _fileUploadServiceMock = new Mock<IFileUploadService>();
        _systemUnderTest = new FileUploadSubsidiaryController(_fileUploadServiceMock.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
    }

    [DataTestMethod]
    public async Task FileUpload_ReturnsBadRequestResult_WhenFilenameHeaderIsMissing()
    {
        // Arrange / Act
        var result = await _systemUnderTest.FileUploadSubsidiary(string.Empty, SubmissionType.Subsidiary) as BadRequestObjectResult;

        // Assert
        result.Value.As<ValidationProblemDetails>().Errors
            .Should()
            .HaveCount(1)
            .And
            .ContainKey("fileName")
            .WhoseValue
            .Should()
            .BeEquivalentTo("fileName header is required");

        _fileUploadServiceMock.Verify(
            x => x.UploadFileSubsidiaryAsync(
                It.IsAny<Stream>(),
                It.IsAny<SubmissionType>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [DataTestMethod]
    public async Task FileUpload_ReturnsBadRequestResult_WhenSubmissionTypeIsWrong()
    {
        // Arrange / Act
        var result = await _systemUnderTest.FileUploadSubsidiary(Filename, SubmissionType.Producer) as BadRequestObjectResult;

        // Assert
        result.Value.As<ValidationProblemDetails>().Errors
            .Should()
            .HaveCount(1)
            .And
            .ContainKey("submissionType")
            .WhoseValue
            .Should()
            .BeEquivalentTo("submissionType header must be subsidiary");

        _fileUploadServiceMock.Verify(
            x => x.UploadFileSubsidiaryAsync(
                It.IsAny<Stream>(),
                It.IsAny<SubmissionType>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod]
    public async Task FileUploadSubsidiary_ReturnsOkResult_WhenAllRequiredFieldsAreProvided()
    {
        // Arrange
        var filestream = new MemoryStream();
        _systemUnderTest.HttpContext.Request.Body = filestream;

        _fileUploadServiceMock.Setup(x => x.UploadFileSubsidiaryAsync(
            It.IsAny<Stream>(),
            It.IsAny<SubmissionType>(),
            It.IsAny<string>()))
            .ReturnsAsync(_submissionId);

        // Act
        var result = await _systemUnderTest.FileUploadSubsidiary(Filename, SubmissionType.Subsidiary) as CreatedAtRouteResult;

        // Assert
        result.RouteName.Should()
            .Be(nameof(SubmissionController.GetSubmission));
        result.RouteValues.Should()
            .HaveCount(1)
            .And
            .ContainKey("submissionId")
            .WhoseValue
            .Should()
            .Be(_submissionId);

        _fileUploadServiceMock.Verify(
            x => x.UploadFileSubsidiaryAsync(
                filestream,
                SubmissionType.Subsidiary,
                Filename),
            Times.Once);
    }
}
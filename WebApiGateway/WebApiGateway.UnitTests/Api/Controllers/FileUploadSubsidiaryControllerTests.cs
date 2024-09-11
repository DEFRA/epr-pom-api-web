namespace WebApiGateway.UnitTests.Api.Controllers;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Controllers;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.Subsidiaries;

[TestClass]
public class FileUploadSubsidiaryControllerTests
{
    private const string Filename = "filename.csv";
    private readonly Guid _submissionId = Guid.NewGuid();
    private Mock<IFileUploadService> _fileUploadServiceMock;
    private Mock<ISubsidiariesService> _subsidiariesServiceMock;
    private FileUploadSubsidiaryController _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _fileUploadServiceMock = new Mock<IFileUploadService>();
        _subsidiariesServiceMock = new Mock<ISubsidiariesService>();
        _systemUnderTest = new FileUploadSubsidiaryController(_fileUploadServiceMock.Object, _subsidiariesServiceMock.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
    }

    [TestMethod]
    public async Task GetFileUploadTemplateAsync_ReturnsFile_WhenFileExists()
    {
        // Arrange
        var expectedResponse = new GetFileUploadTemplateResponse
        {
            Name = "temaplte.ods",
            ContentType = "application/vnd.oasis.opendocument.spreadsheet",
            Content = new MemoryStream()
        };

        _subsidiariesServiceMock.Setup(x => x.GetFileUploadTemplateAsync()).ReturnsAsync(expectedResponse);

        // Act
        var result = await _systemUnderTest.GetFileUploadTemplateAsync() as FileStreamResult;

        // Assert
        _subsidiariesServiceMock.Verify(x => x.GetFileUploadTemplateAsync(), Times.Once);

        result.FileDownloadName.Should().Be(expectedResponse.Name);
        result.ContentType.Should().Be(expectedResponse.ContentType);
        result.FileStream.Should().BeSameAs(expectedResponse.Content);
    }

    [TestMethod]
    public async Task GetFileUploadTemplate_ReturnsNotFound_WhenFileDoesNotExist()
    {
        // Arrange
        _subsidiariesServiceMock.Setup(x => x.GetFileUploadTemplateAsync()).ReturnsAsync((GetFileUploadTemplateResponse)null);

        // Act
        var result = await _systemUnderTest.GetFileUploadTemplateAsync();

        // Assert
        _subsidiariesServiceMock.Verify(x => x.GetFileUploadTemplateAsync(), Times.Once);

        result.Should().BeOfType<NotFoundResult>();
    }

    [DataTestMethod]
    public async Task FileUpload_ReturnsBadRequestResult_WhenFilenameHeaderIsMissing()
    {
        // Arrange / Act
        var result = await _systemUnderTest.FileUploadSubsidiary(string.Empty, SubmissionType.Subsidiary, null) as BadRequestObjectResult;

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
                It.IsAny<string>(),
                null),
            Times.Never);
    }

    [DataTestMethod]
    public async Task FileUpload_ReturnsBadRequestResult_WhenSubmissionTypeIsWrong()
    {
        // Arrange / Act
        var result = await _systemUnderTest.FileUploadSubsidiary(Filename, SubmissionType.Producer, null) as BadRequestObjectResult;

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
                It.IsAny<string>(),
                null),
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
            It.IsAny<string>(),
            null))
            .ReturnsAsync(_submissionId);

        // Act
        var result = await _systemUnderTest.FileUploadSubsidiary(Filename, SubmissionType.Subsidiary, null) as CreatedAtRouteResult;

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
                Filename,
                null),
            Times.Once);
    }

    [TestMethod]
    public async Task FileUploadSubsidiary_ReturnsOkResult_WhenAllComplianceSchemeIdIsValid()
    {
        // Arrange
        var filestream = new MemoryStream();
        _systemUnderTest.HttpContext.Request.Body = filestream;
        var complianceSchemeId = Guid.NewGuid();

        _fileUploadServiceMock.Setup(x => x.UploadFileSubsidiaryAsync(
            It.IsAny<Stream>(),
            It.IsAny<SubmissionType>(),
            It.IsAny<string>(),
            complianceSchemeId))
            .ReturnsAsync(_submissionId);

        // Act
        var result = await _systemUnderTest.FileUploadSubsidiary(Filename, SubmissionType.Subsidiary, complianceSchemeId) as CreatedAtRouteResult;

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
                Filename,
                complianceSchemeId),
            Times.Once);
    }
}
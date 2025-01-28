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
public class FileUploadControllerTests
{
    private const string Filename = "filename.csv";
    private const string SubmissionPeriod = "Jan to Jun 23";
    private readonly Guid _submissionId = Guid.NewGuid();
    private readonly Guid _registrationSetId = Guid.NewGuid();
    private Mock<IFileUploadService> _fileUploadServiceMock;
    private FileUploadController _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _fileUploadServiceMock = new Mock<IFileUploadService>();
        _systemUnderTest = new FileUploadController(_fileUploadServiceMock.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
    }

    [TestMethod]
    public async Task FileUpload_ReturnsBadRequestObjectResult_WhenSubmissionSubTypeIsNull()
    {
        // Arrange / Act
        var result = await _systemUnderTest.FileUpload(Filename, SubmissionType.Registration, null, _registrationSetId, SubmissionPeriod, null, null) as BadRequestObjectResult;

        // Assert
        result.Value.As<ValidationProblemDetails>().Errors
            .Should()
            .HaveCount(1)
            .And
            .ContainKey("submissionSubType")
            .WhoseValue
            .Should()
            .BeEquivalentTo("submissionSubType header is required");

        _fileUploadServiceMock.Verify(
            x => x.UploadFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<SubmissionType>(),
                It.IsAny<SubmissionSubType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [DataTestMethod]
    [DataRow(SubmissionSubType.Brands)]
    [DataRow(SubmissionSubType.Partnerships)]
    public async Task FileUpload_ReturnsBadRequestObjectResult_WhenSubmissionIdHeaderIsMissing(SubmissionSubType submissionSubType)
    {
        // Arrange / Act
        var result = await _systemUnderTest.FileUpload(Filename, SubmissionType.Registration, submissionSubType, _registrationSetId, SubmissionPeriod, null, null) as BadRequestObjectResult;

        // Assert
        result.Value.As<ValidationProblemDetails>().Errors
            .Should()
            .HaveCount(1)
            .And
            .ContainKey("submissionId")
            .WhoseValue
            .Should()
            .BeEquivalentTo("submissionId header is required");

        _fileUploadServiceMock.Verify(
            x => x.UploadFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<SubmissionType>(),
                It.IsAny<SubmissionSubType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [TestMethod]
    public async Task FileUpload_ReturnsBadRequestObjectResult_WhenRegistrationSetIdIsNull()
    {
        // Arrange / Act
        var result = await _systemUnderTest.FileUpload(Filename, SubmissionType.Registration, SubmissionSubType.CompanyDetails, null, SubmissionPeriod, null, null) as BadRequestObjectResult;

        // Assert
        result.Value.As<ValidationProblemDetails>().Errors
            .Should()
            .HaveCount(1)
            .And
            .ContainKey("registrationSetId")
            .WhoseValue
            .Should()
            .BeEquivalentTo("registrationSetId header is required");

        _fileUploadServiceMock.Verify(
            x => x.UploadFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<SubmissionType>(),
                It.IsAny<SubmissionSubType>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [TestMethod]
    public async Task FileUpload_ReturnsCreatedAtRouteResult_WhenAllRequiredFieldsAreProvided()
    {
        // Arrange
        var filestream = new MemoryStream();
        _systemUnderTest.HttpContext.Request.Body = filestream;

        _fileUploadServiceMock.Setup(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<SubmissionType>(),
            null,
            It.IsAny<string>(),
            It.IsAny<string>(),
            null,
            null,
            null))
            .ReturnsAsync(_submissionId);

        // Act
        var result = await _systemUnderTest.FileUpload(Filename, SubmissionType.Producer, null, null, SubmissionPeriod, null, null) as CreatedAtRouteResult;

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
            x => x.UploadFileAsync(
                filestream,
                SubmissionType.Producer,
                null,
                Filename,
                SubmissionPeriod,
                null,
                null,
                null),
            Times.Once);
    }
}
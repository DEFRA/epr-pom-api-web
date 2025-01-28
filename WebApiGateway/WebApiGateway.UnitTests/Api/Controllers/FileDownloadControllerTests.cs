using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Constants;
using WebApiGateway.Api.Controllers;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.FileDownload;

namespace WebApiGateway.UnitTests.Api.Controllers;

[TestClass]
public class FileDownloadControllerTests
{
    private const string Filename = "filename.csv";
    private readonly Guid _fileId = Guid.NewGuid();
    private readonly Guid _submissionId = Guid.NewGuid();
    private Mock<IFileDownloadService> _fileDownloadServiceMock;
    private FileDownloadController _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _fileDownloadServiceMock = new Mock<IFileDownloadService>();
        var mockHttpContext = new Mock<HttpContext>();

        _systemUnderTest = new FileDownloadController(_fileDownloadServiceMock.Object)
        {
            ControllerContext = { HttpContext = mockHttpContext.Object }
        };
    }

    [TestMethod]
    public async Task Get_ThrowsException_WhenUnableToDownloadFile()
    {
        // Arrange
        var exceptionMessage = $"Unable to download file belonging to fileId: {_fileId}.";
        var exception = new Exception(exceptionMessage);
        _fileDownloadServiceMock
            .Setup(x => x.DownloadFileAsync(_fileId, Filename, SubmissionType.Registration, _submissionId))
            .ThrowsAsync(exception);

        // Act
        Func<Task> act = async () => await _systemUnderTest.Get(Filename, _fileId, SubmissionType.Registration, _submissionId);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage(exceptionMessage);
    }

    [TestMethod]
    public async Task Get_ReturnsErrorMessage_WhenFileIsInfected()
    {
        // Arrange
        var data = new FileDownloadData
        {
            Stream = new MemoryStream(Encoding.UTF8.GetBytes("test_data")),
            AntiVirusResult = ContentScan.Malicious,
        };
        _fileDownloadServiceMock.Setup(x => x.DownloadFileAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<SubmissionType>(),
            It.IsAny<Guid>()))
            .ReturnsAsync(data);

        // Act
        var result = await _systemUnderTest.Get(Filename, _fileId, SubmissionType.Registration, _submissionId) as ObjectResult;

        // Assert
        result.Should().BeOfType<ObjectResult>();
        result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [TestMethod]
    public async Task Get_ReturnsFileContentResult_WheFileDownloadWasSuccessful()
    {
        // Arrange
        var data = new FileDownloadData
        {
            Stream = new MemoryStream(Encoding.UTF8.GetBytes("test_data")),
            AntiVirusResult = ContentScan.Clean,
        };
        _fileDownloadServiceMock.Setup(x => x.DownloadFileAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<SubmissionType>(),
            It.IsAny<Guid>()))
            .ReturnsAsync(data);

        // Act
        var result = await _systemUnderTest.Get(Filename, _fileId, SubmissionType.Registration, _submissionId);

        // Assert
        result.Should().BeOfType<FileContentResult>();
        _fileDownloadServiceMock.Verify(
            x => x.DownloadFileAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<SubmissionType>(),
                It.IsAny<Guid>()),
            Times.Once);
    }
}
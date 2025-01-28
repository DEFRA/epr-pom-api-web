namespace WebApiGateway.UnitTests.Api.Services;

using System.Net.Http;
using Azure.Storage.Blobs;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Constants;
using WebApiGateway.Api.Services;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Options;

[TestClass]
public class FileDownloadServiceTests
{
    private const string Filename = "filename.csv";
    private readonly Guid _fileId = Guid.NewGuid();
    private readonly Guid _submissionId = Guid.NewGuid();
    private FileDownloadService _systemUnderTest;
    private Mock<BlobServiceClient> _blobServiceClient;
    private Mock<IAntivirusService> _antivirusServiceMock;
    private Mock<ISubmissionService> _submissionServiceMock;

    [TestInitialize]
    public void TestInitialize()
    {
        _blobServiceClient = new Mock<BlobServiceClient>();
        _submissionServiceMock = new Mock<ISubmissionService>();
        _antivirusServiceMock = new Mock<IAntivirusService>();
        var options = Options.Create(new StorageAccountOptions { });

        var blobContainerClient = new Mock<BlobContainerClient>();
        var blobClient = new Mock<BlobClient>();
        blobContainerClient.Setup(x => x.GetBlobClient(
            It.IsAny<string>()))
            .Returns(blobClient.Object);
        _blobServiceClient.Setup(x => x.GetBlobContainerClient(
            It.IsAny<string>()))
            .Returns(blobContainerClient.Object);

        _systemUnderTest = new FileDownloadService(_blobServiceClient.Object, _submissionServiceMock.Object, _antivirusServiceMock.Object, options);
    }

    [TestMethod]
    public async Task DownloadFileAsync_ShouldReturnStream_WhenFileDownloadSuccessful()
    {
        // Arrange
        var stringContent = new StringContent(ContentScan.Clean);
        var sendFileResponse = new HttpResponseMessage
        {
            Content = stringContent
        };
        _antivirusServiceMock.Setup(x => x.SendFileAndScanAsync(
            It.IsAny<SubmissionType>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<MemoryStream>()))
            .ReturnsAsync(sendFileResponse);

        // Act
        var result = await _systemUnderTest.DownloadFileAsync(_fileId, Filename, SubmissionType.Registration, _submissionId);

        // Assert
        result.Stream.Should().NotBeNull();
        result.AntiVirusResult.Should().Be(ContentScan.Clean);
    }

    [TestMethod]
    public async Task DownloadFileAsync_ThrowsException_WhenUnableToDownloadFile()
    {
        // Arrange
        _blobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>())).Throws<Exception>();

        // Act / Assert
        await _systemUnderTest.Invoking(x => x.DownloadFileAsync(_fileId, Filename, SubmissionType.Registration, _submissionId)).Should().ThrowAsync<Exception>();
    }

    [TestMethod]
    public async Task DownloadFileAsync_ShouldReturnMaliciousContentScan_WhenFileIsInfected()
    {
        // Arrange
        var stringContent = new StringContent(ContentScan.Malicious);
        var sendFileResponse = new HttpResponseMessage
        {
            Content = stringContent
        };
        _antivirusServiceMock.Setup(x => x.SendFileAndScanAsync(
            It.IsAny<SubmissionType>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<MemoryStream>()))
            .ReturnsAsync(sendFileResponse);

        // Act
        var result = await _systemUnderTest.DownloadFileAsync(_fileId, Filename, SubmissionType.Registration, _submissionId);

        // Assert
        result.AntiVirusResult.Should().Be(ContentScan.Malicious);
    }
}
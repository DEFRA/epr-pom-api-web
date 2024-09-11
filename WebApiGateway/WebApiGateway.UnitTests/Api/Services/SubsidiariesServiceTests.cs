using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Options;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class SubsidiariesServiceTests
{
    private Mock<BlobServiceClient> _blobServiceClient;
    private Mock<BlobContainerClient> _blobContainerClient;
    private Mock<BlobClient> _blobClient;
    private BlobStorageOptions _blobStorageOptions;
    private SubsidiariesService _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _blobServiceClient = new Mock<BlobServiceClient>();
        _blobContainerClient = new Mock<BlobContainerClient>();
        _blobClient = new Mock<BlobClient>();
        _blobStorageOptions = new BlobStorageOptions
        {
            SubsidiariesContainerName = "test-container",
            SubsidiariesFileUploadTemplateFileName = "test-file.csv"
        };

        _blobServiceClient.Setup(x => x.GetBlobContainerClient(_blobStorageOptions.SubsidiariesContainerName))
            .Returns(_blobContainerClient.Object);

        _blobClient.Setup(x => x.Name).Returns(_blobStorageOptions.SubsidiariesFileUploadTemplateFileName);

        _blobContainerClient.Setup(x => x.GetBlobClient(_blobStorageOptions.SubsidiariesFileUploadTemplateFileName))
            .Returns(_blobClient.Object);

        _systemUnderTest = new SubsidiariesService(
            _blobServiceClient.Object,
            Options.Create<BlobStorageOptions>(_blobStorageOptions),
            Mock.Of<ILogger<SubsidiariesService>>());
    }

    [TestMethod]
    public async Task GetFileUploadTemplateAsync_ReturnsFile_WhenBlobExists()
    {
        // Arrange
        var expectedName = _blobStorageOptions.SubsidiariesFileUploadTemplateFileName;
        var expectedContentType = "text/csv";
        var expectedBytes = new byte[] { 1 };

        var responseMock = Mock.Of<Response>();

        _blobClient.Setup(x => x.GetPropertiesAsync(null, CancellationToken.None))
            .ReturnsAsync(Response.FromValue<BlobProperties>(BlobsModelFactory.BlobProperties(contentType: expectedContentType), responseMock));

        _blobClient.Setup(x => x.DownloadContentAsync())
            .ReturnsAsync(Response.FromValue<BlobDownloadResult>(BlobsModelFactory.BlobDownloadResult(content: new BinaryData(expectedBytes)), responseMock));

        // Act
        var result = await _systemUnderTest.GetFileUploadTemplateAsync();

        // Assert
        result.Name.Should().Be(expectedName);
        result.ContentType.Should().Be(expectedContentType);
        result.Content.ReadByte().Should().Be(expectedBytes[0]);
        result.Content.ReadByte().Should().Be(-1);

        _blobClient.Verify(x => x.GetPropertiesAsync(null, CancellationToken.None), Times.Once);
        _blobClient.Verify(x => x.DownloadContentAsync(), Times.Once);
    }

    [TestMethod]
    public async Task GetFileUploadTemplateAsync_ReturnsNull_WhenBlobPropertiesNotFound()
    {
        // Arrange
        var responseMock = new Mock<Response>();
        responseMock.Setup(x => x.Status).Returns(404);

        _blobClient.Setup(x => x.GetPropertiesAsync(null, CancellationToken.None))
            .ThrowsAsync(new RequestFailedException(responseMock.Object));

        // Act
        var result = await _systemUnderTest.GetFileUploadTemplateAsync();

        // Assert
        result.Should().BeNull();

        _blobClient.Verify(x => x.GetPropertiesAsync(null, CancellationToken.None), Times.Once);
    }

    [TestMethod]
    public async Task GetFileUploadTemplateAsync_ThrowsException_WhenBlobPropertiesRequestFails()
    {
        // Arrange
        var responseMock = new Mock<Response>();

        _blobClient.Setup(x => x.GetPropertiesAsync(null, CancellationToken.None))
            .ThrowsAsync(new RequestFailedException(responseMock.Object));

        // Act
        var act = async () => await _systemUnderTest.GetFileUploadTemplateAsync();

        // Assert
        act.Should().ThrowAsync<RequestFailedException>();

        _blobClient.Verify(x => x.GetPropertiesAsync(null, CancellationToken.None), Times.Once);
    }

    [TestMethod]
    public async Task GetFileUploadTemplateAsync_ReturnsNull_DownloadContentNotFound()
    {
        // Arrange
        var notFoundResponse = new Mock<Response>();
        notFoundResponse.Setup(x => x.Status).Returns(404);

        _blobClient.Setup(x => x.GetPropertiesAsync(null, CancellationToken.None))
            .ReturnsAsync(Response.FromValue<BlobProperties>(BlobsModelFactory.BlobProperties(), Mock.Of<Response>()));

        _blobClient.Setup(x => x.DownloadContentAsync()).ThrowsAsync(new RequestFailedException(notFoundResponse.Object));

        // Act
        var result = await _systemUnderTest.GetFileUploadTemplateAsync();

        // Assert
        result.Should().BeNull();

        _blobClient.Verify(x => x.GetPropertiesAsync(null, CancellationToken.None), Times.Once);
        _blobClient.Verify(x => x.DownloadContentAsync(), Times.Once);
    }

    [TestMethod]
    public async Task GetFileUploadTemplateAsync_ThrowsException_WhenDownloadContentRequestFails()
    {
        // Arrange
        var responseMock = Mock.Of<Response>();

        _blobClient.Setup(x => x.GetPropertiesAsync(null, CancellationToken.None))
            .ReturnsAsync(Response.FromValue<BlobProperties>(BlobsModelFactory.BlobProperties(), responseMock));

        _blobClient.Setup(x => x.DownloadContentAsync()).ThrowsAsync(new RequestFailedException(responseMock));

        // Act
        var act = async () => await _systemUnderTest.GetFileUploadTemplateAsync();

        // Assert
        act.Should().ThrowAsync<RequestFailedException>();

        _blobClient.Verify(x => x.GetPropertiesAsync(null, CancellationToken.None), Times.Once);
        _blobClient.Verify(x => x.DownloadContentAsync(), Times.Once);
    }
}

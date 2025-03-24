using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Services;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class FileUploadServiceTests
{
    private const string Filename = "filename.csv";
    private const string SubmissionPeriod = "Jan to Jun 23";
    private readonly Guid _fileId = Guid.NewGuid();
    private FileUploadService _systemUnderTest;
    private Mock<IAntivirusService> _antivirusServiceMock;
    private Mock<ISubmissionService> _submissionServiceMock;

    [TestInitialize]
    public void TestInitialize()
    {
        _antivirusServiceMock = new Mock<IAntivirusService>();
        _submissionServiceMock = new Mock<ISubmissionService>();
        _systemUnderTest = new FileUploadService(_submissionServiceMock.Object, _antivirusServiceMock.Object);
    }

    [TestMethod]
    public async Task UploadFileAsync_CreatesNewSubmissionAndAntivirusCheckEvent_WhenUploadingPomFileWithNoPreviousSubmissionId()
    {
        // Arrange
        var fileStream = new MemoryStream();
        var submissionId = Guid.NewGuid();

        _submissionServiceMock
            .Setup(x => x.CreateSubmissionAsync(It.IsAny<SubmissionType>(), It.IsAny<string>(), null, null))
            .ReturnsAsync(submissionId);
        _submissionServiceMock
            .Setup(x => x.CreateAntivirusCheckEventAsync(It.IsAny<string>(), It.IsAny<FileType>(), It.IsAny<Guid>(), null))
            .ReturnsAsync(_fileId);

        // Act
        var result = await _systemUnderTest.UploadFileAsync(fileStream, SubmissionType.Producer, null, Filename, SubmissionPeriod, null, null, null, null);

        // Assert
        result.Should().Be(submissionId);

        _submissionServiceMock.Verify(x => x.CreateSubmissionAsync(SubmissionType.Producer, SubmissionPeriod, null, null), Times.Once);
        _submissionServiceMock.Verify(x => x.CreateAntivirusCheckEventAsync(Filename, FileType.Pom, submissionId, null), Times.Once);
        _antivirusServiceMock.Verify(x => x.SendFileAsync(SubmissionType.Producer, _fileId, Filename, fileStream), Times.Once);
    }

    [TestMethod]
    public async Task UploadFileAsync_CreatesNewSubmissionAndAntivirusCheckEvent_WhenUploadingCompanyDetailsFileWithNoPreviousSubmissionId()
    {
        // Arrange
        var fileStream = new MemoryStream();
        var submissionId = Guid.NewGuid();

        _submissionServiceMock
            .Setup(x => x.CreateSubmissionAsync(It.IsAny<SubmissionType>(), It.IsAny<string>(), null, true))
            .ReturnsAsync(submissionId);
        _submissionServiceMock
            .Setup(x => x.CreateAntivirusCheckEventAsync(It.IsAny<string>(), It.IsAny<FileType>(), It.IsAny<Guid>(), null))
            .ReturnsAsync(_fileId);

        // Act
        var result = await _systemUnderTest.UploadFileAsync(fileStream, SubmissionType.Registration, SubmissionSubType.CompanyDetails, Filename, SubmissionPeriod, null, null, null, true);

        // Assert
        result.Should().Be(submissionId);

        _submissionServiceMock.Verify(x => x.CreateSubmissionAsync(SubmissionType.Registration, SubmissionPeriod, null, true), Times.Once);
        _submissionServiceMock.Verify(x => x.CreateAntivirusCheckEventAsync(Filename, FileType.CompanyDetails, submissionId, null), Times.Once);
        _antivirusServiceMock.Verify(x => x.SendFileAsync(SubmissionType.Registration, _fileId, Filename, fileStream), Times.Once);
    }

    [TestMethod]
    public async Task UploadFileAsync_CreatesOnlyAntivirusCheckEvent_WhenReUploadingPomFileWithPreviousSubmissionId()
    {
        // Arrange
        const SubmissionType SubmissionType = SubmissionType.Producer;
        var fileStream = new MemoryStream();
        var originalSubmissionId = Guid.NewGuid();

        _submissionServiceMock
            .Setup(x => x.CreateAntivirusCheckEventAsync(It.IsAny<string>(), It.IsAny<FileType>(), It.IsAny<Guid>(), null))
            .ReturnsAsync(_fileId);

        // Act
        var result = await _systemUnderTest.UploadFileAsync(fileStream, SubmissionType, null, Filename, SubmissionPeriod, originalSubmissionId, null, null, false);

        // Assert
        result.Should().Be(originalSubmissionId);

        _submissionServiceMock.Verify(x => x.CreateSubmissionAsync(SubmissionType, SubmissionPeriod, It.IsAny<Guid>(), false), Times.Never);
        _submissionServiceMock.Verify(x => x.CreateAntivirusCheckEventAsync(Filename, FileType.Pom, originalSubmissionId, null), Times.Once);
        _antivirusServiceMock.Verify(x => x.SendFileAsync(SubmissionType, _fileId, Filename, fileStream), Times.Once);
    }

    [TestMethod]
    public async Task UploadFileAsync_CreatesOnlyAntivirusCheckEvent_WhenReUploadingCompanyDetailsFileWithPreviousSubmissionId()
    {
        // Arrange
        const SubmissionType SubmissionType = SubmissionType.Registration;
        const SubmissionSubType SubmissionSubType = SubmissionSubType.CompanyDetails;
        var fileStream = new MemoryStream();
        var originalSubmissionId = Guid.NewGuid();
        var registrationSetId = Guid.NewGuid();

        _submissionServiceMock
            .Setup(x => x.CreateAntivirusCheckEventAsync(It.IsAny<string>(), It.IsAny<FileType>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(_fileId);

        // Act
        var result = await _systemUnderTest.UploadFileAsync(fileStream, SubmissionType, SubmissionSubType, Filename, SubmissionPeriod, originalSubmissionId, registrationSetId, null, null);

        // Assert
        result.Should().Be(originalSubmissionId);

        _submissionServiceMock.Verify(x => x.CreateSubmissionAsync(It.IsAny<SubmissionType>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool?>()), Times.Never);
        _submissionServiceMock.Verify(x => x.CreateAntivirusCheckEventAsync(Filename, FileType.CompanyDetails, originalSubmissionId, registrationSetId), Times.Once);
        _antivirusServiceMock.Verify(x => x.SendFileAsync(SubmissionType, _fileId, Filename, fileStream), Times.Once);
    }

    [TestMethod]
    [DataRow(SubmissionSubType.Brands, FileType.Brands)]
    [DataRow(SubmissionSubType.Partnerships, FileType.Partnerships)]
    public async Task UploadFileAsync_CreatesOnlyAntivirusCheckEvent_WhenUploadingAdditionalRegistrationFile(
        SubmissionSubType submissionSubType,
        FileType expectedFileType)
    {
        // Arrange
        const SubmissionType SubmissionType = SubmissionType.Registration;
        var fileStream = new MemoryStream();
        var originalSubmissionId = Guid.NewGuid();
        var registrationSetId = Guid.NewGuid();

        _submissionServiceMock
            .Setup(x => x.CreateAntivirusCheckEventAsync(It.IsAny<string>(), It.IsAny<FileType>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(_fileId);

        // Act
        var result = await _systemUnderTest.UploadFileAsync(fileStream, SubmissionType, submissionSubType, Filename, SubmissionPeriod, originalSubmissionId, registrationSetId, null, null);

        // Assert
        result.Should().Be(originalSubmissionId);

        _submissionServiceMock.Verify(x => x.CreateSubmissionAsync(It.IsAny<SubmissionType>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool?>()), Times.Never);
        _submissionServiceMock.Verify(x => x.CreateAntivirusCheckEventAsync(Filename, expectedFileType, originalSubmissionId, registrationSetId), Times.Once);
        _antivirusServiceMock.Verify(x => x.SendFileAsync(SubmissionType, _fileId, Filename, fileStream), Times.Once);
    }

    [TestMethod]
    public async Task UploadFileSubsidiaryAsync_CallsSendFile_WhenUploadingSubsidiaryFile()
    {
        // Arrange
        var fileStream = new MemoryStream();

        // Act
        await _systemUnderTest.UploadFileSubsidiaryAsync(fileStream, SubmissionType.Subsidiary, Filename, null);

        // Assert
        _antivirusServiceMock.Verify(x => x.SendFileAsync(SubmissionType.Subsidiary, It.IsAny<Guid>(), Filename, fileStream), Times.Once);
    }
}
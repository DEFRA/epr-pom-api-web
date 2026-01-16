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
    private FileUploadService _fileUploadService;
    private Mock<IAntivirusService> _antivirusServiceMock;
    private Mock<ISubmissionService> _submissionServiceMock;

    [TestInitialize]
    public void TestInitialize()
    {
        _antivirusServiceMock = new Mock<IAntivirusService>();
        _submissionServiceMock = new Mock<ISubmissionService>();
        _fileUploadService = new FileUploadService(_submissionServiceMock.Object, _antivirusServiceMock.Object);
    }

    [TestMethod]
    public async Task UploadFileAsync_CreatesNewSubmissionAndAntivirusCheckEvent_WhenUploadingPomFileWithNoPreviousSubmissionId()
    {
        // Arrange
        var fileStream = new MemoryStream();
        var submissionId = Guid.NewGuid();

        _submissionServiceMock
            .Setup(x => x.CreateSubmissionAsync(It.IsAny<SubmissionType>(), It.IsAny<string>(), null, null, It.IsAny<string>()))
            .ReturnsAsync(submissionId);
        _submissionServiceMock
            .Setup(x => x.CreateAntivirusCheckEventAsync(It.IsAny<string>(), It.IsAny<FileType>(), It.IsAny<Guid>(), null))
            .ReturnsAsync(_fileId);
        var fileUploadDetails = new FileUploadDetails(SubmissionType.Producer, null, Filename, SubmissionPeriod, null, null, null);
        fileUploadDetails.IsResubmission = null;
        fileUploadDetails.RegistrationJourney = "journey";

        // Act
        var result = await _fileUploadService.UploadFileAsync(fileStream, fileUploadDetails);

        // Assert
        result.Should().Be(submissionId);

        _submissionServiceMock.Verify(x => x.CreateSubmissionAsync(SubmissionType.Producer, SubmissionPeriod, null, null, "journey"), Times.Once);
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
            .Setup(x => x.CreateSubmissionAsync(It.IsAny<SubmissionType>(), It.IsAny<string>(), null, true, It.IsAny<string>()))
            .ReturnsAsync(submissionId);
        _submissionServiceMock
            .Setup(x => x.CreateAntivirusCheckEventAsync(It.IsAny<string>(), It.IsAny<FileType>(), It.IsAny<Guid>(), null))
            .ReturnsAsync(_fileId);
        var fileUploadDetails = new FileUploadDetails(SubmissionType.Registration, SubmissionSubType.CompanyDetails, Filename, SubmissionPeriod, null, null, null);
        fileUploadDetails.IsResubmission = true;
        fileUploadDetails.RegistrationJourney = "journey";
        
        var result = await _fileUploadService.UploadFileAsync(fileStream, fileUploadDetails);

        result.Should().Be(submissionId);
        _submissionServiceMock.Verify(x => x.CreateSubmissionAsync(SubmissionType.Registration, SubmissionPeriod, null, true, "journey"), Times.Once);
        _submissionServiceMock.Verify(x => x.CreateAntivirusCheckEventAsync(Filename, FileType.CompanyDetails, submissionId, null), Times.Once);
        _antivirusServiceMock.Verify(x => x.SendFileAsync(SubmissionType.Registration, _fileId, Filename, fileStream), Times.Once);
    }

    [TestMethod]
    public async Task UploadFileAsync_CreatesOnlyAntivirusCheckEvent_WhenReUploadingPomFileWithPreviousSubmissionId()
    {
        const SubmissionType SubmissionType = SubmissionType.Producer;
        var fileStream = new MemoryStream();
        var originalSubmissionId = Guid.NewGuid();

        _submissionServiceMock
            .Setup(x => x.CreateAntivirusCheckEventAsync(It.IsAny<string>(), It.IsAny<FileType>(), It.IsAny<Guid>(), null))
            .ReturnsAsync(_fileId);
        var fileUploadDetails = new FileUploadDetails(SubmissionType, null, Filename, SubmissionPeriod, originalSubmissionId, null, null);
        fileUploadDetails.IsResubmission = false;
        fileUploadDetails.RegistrationJourney = "journey";
        
        var result = await _fileUploadService.UploadFileAsync(fileStream, fileUploadDetails);

        result.Should().Be(originalSubmissionId);
        _submissionServiceMock.Verify(x => x.CreateSubmissionAsync(SubmissionType, SubmissionPeriod, It.IsAny<Guid>(), false, "journey"), Times.Never);
        _submissionServiceMock.Verify(x => x.CreateAntivirusCheckEventAsync(Filename, FileType.Pom, originalSubmissionId, null), Times.Once);
        _antivirusServiceMock.Verify(x => x.SendFileAsync(SubmissionType, _fileId, Filename, fileStream), Times.Once);
    }

    [TestMethod]
    public async Task UploadFileAsync_CreatesOnlyAntivirusCheckEvent_WhenReUploadingCompanyDetailsFileWithPreviousSubmissionId()
    {
        const SubmissionType SubmissionType = SubmissionType.Registration;
        const SubmissionSubType SubmissionSubType = SubmissionSubType.CompanyDetails;
        var fileStream = new MemoryStream();
        var originalSubmissionId = Guid.NewGuid();
        var registrationSetId = Guid.NewGuid();

        _submissionServiceMock
            .Setup(x => x.CreateAntivirusCheckEventAsync(It.IsAny<string>(), It.IsAny<FileType>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(_fileId);
        var fileUploadDetails = new FileUploadDetails(SubmissionType, SubmissionSubType, Filename, SubmissionPeriod, originalSubmissionId, registrationSetId, null);
        fileUploadDetails.IsResubmission = null;
        fileUploadDetails.RegistrationJourney = "journey";

        var result = await _fileUploadService.UploadFileAsync(fileStream, fileUploadDetails);

        result.Should().Be(originalSubmissionId);
        _submissionServiceMock.Verify(x => x.CreateSubmissionAsync(It.IsAny<SubmissionType>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool?>(), "journey"), Times.Never);
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
        const SubmissionType SubmissionType = SubmissionType.Registration;
        var fileStream = new MemoryStream();
        var originalSubmissionId = Guid.NewGuid();
        var registrationSetId = Guid.NewGuid();

        _submissionServiceMock
            .Setup(x => x.CreateAntivirusCheckEventAsync(It.IsAny<string>(), It.IsAny<FileType>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(_fileId);
        var fileUploadDetails = new FileUploadDetails(SubmissionType, submissionSubType, Filename, SubmissionPeriod, originalSubmissionId, registrationSetId, null);
        fileUploadDetails.IsResubmission = null;
        fileUploadDetails.RegistrationJourney = "journey";

        var result = await _fileUploadService.UploadFileAsync(fileStream, fileUploadDetails);

        result.Should().Be(originalSubmissionId);
        _submissionServiceMock.Verify(x => x.CreateSubmissionAsync(It.IsAny<SubmissionType>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool?>(), "journey"), Times.Never);
        _submissionServiceMock.Verify(x => x.CreateAntivirusCheckEventAsync(Filename, expectedFileType, originalSubmissionId, registrationSetId), Times.Once);
        _antivirusServiceMock.Verify(x => x.SendFileAsync(SubmissionType, _fileId, Filename, fileStream), Times.Once);
    }

    [TestMethod]
    public async Task UploadFileSubsidiaryAsync_CallsSendFile_WhenUploadingSubsidiaryFile()
    {
        var fileStream = new MemoryStream();

        await _fileUploadService.UploadFileSubsidiaryAsync(fileStream, SubmissionType.Subsidiary, Filename, null, "journey");

        _antivirusServiceMock.Verify(x => x.SendFileAsync(SubmissionType.Subsidiary, It.IsAny<Guid>(), Filename, fileStream), Times.Once);
    }

    [TestMethod]
    public async Task UploadAccreditationAsync_CreatesSubmission_WhenUploadingAccreditationFile()
    {
        const SubmissionType submissionType = SubmissionType.Accreditation;
        var fileStream = new MemoryStream();
        Guid? originalSubmissionId = null;

        _submissionServiceMock
            .Setup(x => x.CreateAntivirusCheckEventAsync(It.IsAny<string>(), It.IsAny<FileType>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(_fileId);

        await _fileUploadService.UploadFileAccreditationAsync(fileStream, submissionType, Filename, originalSubmissionId, "journey");

        _submissionServiceMock.Verify(x => x.CreateSubmissionAsync(It.IsAny<SubmissionType>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<bool?>(), "journey"), Times.Once);
        _submissionServiceMock.Verify(x => x.CreateAntivirusCheckEventAsync(Filename, FileType.Accreditation, It.IsAny<Guid>(), It.IsAny<Guid?>()), Times.Once);
        _antivirusServiceMock.Verify(x => x.SendFileAsync(submissionType, _fileId, Filename, fileStream), Times.Once);
    }

    [TestMethod]
    public async Task UploadFileAccreditationAsync_SendFile_WhenUploadingAccreditationFile()
    {
        // Arrange
        const SubmissionType submissionType = SubmissionType.Accreditation;
        var fileStream = new MemoryStream();
        var originalSubmissionId = Guid.NewGuid();

        _submissionServiceMock
            .Setup(x => x.CreateAntivirusCheckEventAsync(It.IsAny<string>(), It.IsAny<FileType>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
            .ReturnsAsync(_fileId);

        // Act
        var result = await _fileUploadService.UploadFileAccreditationAsync(fileStream, submissionType, Filename, originalSubmissionId, "journey");

        // Assert
        result.Should().Be(originalSubmissionId);

        _submissionServiceMock.Verify(x => x.CreateSubmissionAsync(It.IsAny<SubmissionType>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<bool?>(), "journey"), Times.Never);
        _submissionServiceMock.Verify(x => x.CreateAntivirusCheckEventAsync(Filename, FileType.Accreditation, originalSubmissionId, It.IsAny<Guid?>()), Times.Once);
        _antivirusServiceMock.Verify(x => x.SendFileAsync(submissionType, _fileId, Filename, fileStream), Times.Once);
    }
}
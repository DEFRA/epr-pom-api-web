namespace WebApiGateway.UnitTests.Api.Services;

using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Enumeration;
using WebApiGateway.Core.Models.Antivirus;
using WebApiGateway.Core.Models.UserAccount;
using WebApiGateway.Core.Options;

[TestClass]
public class AntivirusServiceTests
{
    private static readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly UserAccount _userAccount = _fixture.Create<UserAccount>();
    private Mock<IAntivirusClient> _antivirusClientMock;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock;

    [TestInitialize]
    public void TestInitialize()
    {
        _antivirusClientMock = new Mock<IAntivirusClient>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.Setup(x => x.Claims).Returns(new List<Claim>
        {
            new(ClaimConstants.ObjectId, _userAccount.User.Id.ToString()),
            new("emails", _userAccount.User.Email)
        });

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(claimsPrincipalMock.Object);
    }

    [TestMethod]
    public async Task SendFileAsync_PassesFileSteamAndDetailToClient_WhenCollectionSuffixAppSettingIsProvided()
    {
        // Arrange
        const SubmissionType SubmissionType = SubmissionType.Producer;
        const string Filename = "filename.csv";
        var fileId = Guid.NewGuid();
        var fileStream = new MemoryStream();
        var options = Options.Create(new AntivirusApiOptions { CollectionSuffix = "dev1" });
        var systemUnderTest = new AntivirusService(_antivirusClientMock.Object, _httpContextAccessorMock.Object, options);

        // Act
        await systemUnderTest.SendFileAsync(SubmissionType, fileId, Filename, fileStream);

        // Assert
        _antivirusClientMock.Verify(
            x => x.SendFileAsync(
                It.Is<FileDetails>(m =>
                    m.Service == "epr"
                    && m.Key == fileId
                    && m.Extension == ".csv"
                    && m.FileName == "filename"
                    && m.Collection == "pomdev1"
                    && m.UserId == _userAccount.User.Id
                    && m.UserEmail == _userAccount.User.Email),
                Filename,
                fileStream),
            Times.Once);
    }

    [TestMethod]
    public async Task SendFileAsync_PassesFileSteamAndDetailToClient_WhenCollectionSuffixAppSettingIsNotProvided()
    {
        // Arrange
        const SubmissionType SubmissionType = SubmissionType.Producer;
        const string Filename = "filename.csv";
        var fileId = Guid.NewGuid();
        var fileStream = new MemoryStream();
        var options = Options.Create(new AntivirusApiOptions());
        var systemUnderTest = new AntivirusService(_antivirusClientMock.Object, _httpContextAccessorMock.Object, options);

        // Act
        await systemUnderTest.SendFileAsync(SubmissionType, fileId, Filename, fileStream);

        // Assert
        _antivirusClientMock.Verify(
            x => x.SendFileAsync(
                It.Is<FileDetails>(m =>
                    m.Service == "epr"
                    && m.Key == fileId
                    && m.Extension == ".csv"
                    && m.FileName == "filename"
                    && m.Collection == "pom"
                    && m.UserId == _userAccount.User.Id
                    && m.UserEmail == _userAccount.User.Email),
                Filename,
                fileStream),
            Times.Once);
    }
}
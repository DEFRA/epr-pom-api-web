using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Models.ComplianceSchemeDetails;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class ComplianceSchemeDetailsServiceTests
{
    private const int OrganisationId = 1234;
    private Mock<IComplianceSchemeDetailsClient> _complianceSchemeDetailsClientMock;
    private Mock<ILogger<ComplianceSchemeDetailsService>> _loggerMock;
    private ComplianceSchemeDetailsService _service;
    private Mock<IHttpContextAccessor> _contextAccessor;

    [TestInitialize]
    public void SetUp()
    {
        _complianceSchemeDetailsClientMock = new Mock<IComplianceSchemeDetailsClient>();
        _loggerMock = new Mock<ILogger<ComplianceSchemeDetailsService>>();
        _contextAccessor = new Mock<IHttpContextAccessor>();
        _service = new ComplianceSchemeDetailsService(_complianceSchemeDetailsClientMock.Object, _loggerMock.Object, _contextAccessor.Object);
    }

    [TestMethod]
    public async Task GetComplianceSchemeDetails_ShouldLogDebugMessage()
    {
        // Arrange
        _complianceSchemeDetailsClientMock
            .Setup(client => client.GetComplianceSchemeDetails(It.IsAny<int>(), It.IsAny<Guid>()))
            .ReturnsAsync((List<GetComplianceSchemeMemberDetailsResponse>)null);

        // Act
        await _service.GetComplianceSchemeDetails(OrganisationId, Guid.NewGuid());

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Get Compliance Scheme Details For Organisation Id {OrganisationId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task GetComplianceSchemeDetails_ShouldCallComplianceSchemeDetailsClientWithCorrectOrganisationId()
    {
        // Arrange
        var complianceSchemeId = Guid.NewGuid();
        var expectedResponse = new List<GetComplianceSchemeMemberDetailsResponse>();
        _complianceSchemeDetailsClientMock
            .Setup(client => client.GetComplianceSchemeDetails(OrganisationId, complianceSchemeId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.GetComplianceSchemeDetails(OrganisationId, complianceSchemeId);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        _complianceSchemeDetailsClientMock.Verify(client => client.GetComplianceSchemeDetails(OrganisationId, complianceSchemeId), Times.Once);
    }

    [TestMethod]
    public async Task GetComplianceSchemeDetails_ShouldReturnNull_WhenClientReturnsNull()
    {
        // Arrange
        var complianceSchemeId = Guid.NewGuid();
        _complianceSchemeDetailsClientMock
            .Setup(client => client.GetComplianceSchemeDetails(OrganisationId, complianceSchemeId))
            .ReturnsAsync((List<GetComplianceSchemeMemberDetailsResponse>)null);

        // Act
        var result = await _service.GetComplianceSchemeDetails(OrganisationId, complianceSchemeId);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetComplianceSchemeDetails_ShouldThrowException_WhenClientThrowsException()
    {
        // Arrange
        var complianceSchemeId = Guid.NewGuid();
        var expectedException = new HttpRequestException("An error occurred while fetching compliance scheme details.");

        _complianceSchemeDetailsClientMock
            .Setup(client => client.GetComplianceSchemeDetails(OrganisationId, complianceSchemeId))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () => await _service.GetComplianceSchemeDetails(OrganisationId, complianceSchemeId);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage(expectedException.Message);
    }

    [TestMethod]
    public async Task GetComplianceSchemeIdAsync_ShouldReturnComplianceSchemeId()
    {
        // Arrange
        var expectedComplianceSchemeId = Guid.Parse("a8c8de60-4e05-4231-90d1-f66387b47d61");

        // Mock HttpContextAccessor to return a valid HttpContext
        var httpContext = new DefaultHttpContext();
        httpContext.Items[ComplianceScheme.ComplianceSchemeId] = expectedComplianceSchemeId; // Add any necessary items if required by your method
        _contextAccessor.Setup(accessor => accessor.HttpContext).Returns(httpContext);

        // Mock the client to return the expected response
        _complianceSchemeDetailsClientMock
            .Setup(client => client.GetComplianceSchemeDetails(It.IsAny<int>(), It.IsAny<Guid>()))
            .ReturnsAsync(new List<GetComplianceSchemeMemberDetailsResponse>
            {
                new GetComplianceSchemeMemberDetailsResponse { MemberId = expectedComplianceSchemeId.ToString() }
            });

        // Act
        var result = await _service.GetComplianceSchemeIdAsync();

        // Assert
        result.Should().Be(expectedComplianceSchemeId);
    }

    [TestMethod]
    public async Task GetComplianceSchemeIdAsync_ShouldReturnNull_WhenNoComplianceSchemeIdFound()
    {
        // Arrange
        _complianceSchemeDetailsClientMock
            .Setup(client => client.GetComplianceSchemeDetails(It.IsAny<int>(), It.IsAny<Guid>()))
            .ReturnsAsync(new List<GetComplianceSchemeMemberDetailsResponse>());

        // Act
        var result = await _service.GetComplianceSchemeIdAsync();

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetComplianceSchemeIdAsync_ShouldReturnGuid_WhenValueIsGuid()
    {
        // Arrange
        var expectedComplianceSchemeId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Items[ComplianceScheme.ComplianceSchemeId] = expectedComplianceSchemeId;
        _contextAccessor.Setup(accessor => accessor.HttpContext).Returns(httpContext);

        // Act
        var result = await _service.GetComplianceSchemeIdAsync();

        // Assert
        result.Should().Be(expectedComplianceSchemeId);
    }

    [TestMethod]
    public async Task GetComplianceSchemeIdAsync_ShouldReturnGuid_WhenValueIsValidStringRepresentationOfGuid()
    {
        // Arrange
        var expectedComplianceSchemeId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Items[ComplianceScheme.ComplianceSchemeId] = expectedComplianceSchemeId.ToString();
        _contextAccessor.Setup(accessor => accessor.HttpContext).Returns(httpContext);

        // Act
        var result = await _service.GetComplianceSchemeIdAsync();

        // Assert
        result.Should().Be(expectedComplianceSchemeId);
    }

    [TestMethod]
    public async Task GetComplianceSchemeIdAsync_ShouldReturnNull_WhenValueIsInvalidString()
    {
        // Arrange
        var invalidString = "InvalidGuidString";
        var httpContext = new DefaultHttpContext();
        httpContext.Items[ComplianceScheme.ComplianceSchemeId] = invalidString;
        _contextAccessor.Setup(accessor => accessor.HttpContext).Returns(httpContext);

        // Act
        var result = await _service.GetComplianceSchemeIdAsync();

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetComplianceSchemeIdAsync_ShouldReturnNull_WhenValueIsOfDifferentType()
    {
        // Arrange
        var invalidTypeValue = 12345; // Example of a different type
        var httpContext = new DefaultHttpContext();
        httpContext.Items[ComplianceScheme.ComplianceSchemeId] = invalidTypeValue;
        _contextAccessor.Setup(accessor => accessor.HttpContext).Returns(httpContext);

        // Act
        var result = await _service.GetComplianceSchemeIdAsync();

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetComplianceSchemeIdAsync_ShouldReturnNull_WhenValueIsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items[ComplianceScheme.ComplianceSchemeId] = null;
        _contextAccessor.Setup(accessor => accessor.HttpContext).Returns(httpContext);

        // Act
        var result = await _service.GetComplianceSchemeIdAsync();

        // Assert
        result.Should().BeNull();
    }
}
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Constants;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class ComplianceSchemeDetailsServiceTests
{
    private ComplianceSchemeDetailsService _service;
    private Mock<IHttpContextAccessor> _contextAccessor;

    [TestInitialize]
    public void SetUp()
    {
        _contextAccessor = new Mock<IHttpContextAccessor>();
        _service = new ComplianceSchemeDetailsService(_contextAccessor.Object);
    }

    [TestMethod]
    public async Task GetComplianceSchemeIdAsync_ShouldReturnComplianceSchemeId()
    {
        // Arrange
        var expectedComplianceSchemeId = Guid.Parse("a8c8de60-4e05-4231-90d1-f66387b47d61");

        // Mock HttpContextAccessor to return a valid HttpContext
        var httpContext = new DefaultHttpContext
        {
            Items =
            {
                [ComplianceScheme.ComplianceSchemeId] = expectedComplianceSchemeId // Add any necessary items if required by your method
            }
        };
        _contextAccessor.Setup(accessor => accessor.HttpContext).Returns(httpContext);

        // Act
        var result = await _service.GetComplianceSchemeIdAsync();

        // Assert
        result.Should().Be(expectedComplianceSchemeId);
    }

    [TestMethod]
    public async Task GetComplianceSchemeIdAsync_ShouldReturnNull_WhenNoComplianceSchemeIdFound()
    {
        // Arrange

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
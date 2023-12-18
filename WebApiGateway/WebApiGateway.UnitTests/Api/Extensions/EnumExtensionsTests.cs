namespace WebApiGateway.UnitTests.Api.Extensions;

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiGateway.Api.Extensions;
using WebApiGateway.Core.Enumeration;

[TestClass]
public class EnumExtensionsTests
{
    [TestMethod]
    public void GetDisplayName_ReturnsDisplayName_WhenDisplayNameAnnotationExists()
    {
        // Arrange / Act
        var result = SubmissionType.Producer.GetDisplayName();

        // Assert
        result.Should().Be("pom");
    }

    [TestMethod]
    public void GetDisplayName_ReturnsNull_WhenDisplayNameAnnotationDoesNotExist()
    {
        // Arrange / Act
        var result = SubmissionSubType.CompanyDetails.GetDisplayName();

        // Assert
        result.Should().BeNull();
    }
}
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiGateway.Api.Clients;

namespace WebApiGateway.UnitTests.Api.Clients;

[TestClass]
public class ServiceClientBaseTests
{
    [TestMethod]
    public void BuildUrlWithQueryString_ShouldReturnCorrectQueryString_WhenDtoHasProperties()
    {
        // Arrange
        var dto = new { Name = "Test", Value = 123 };

        // Act
        var result = ServiceClientBase.BuildUrlWithQueryString(dto);

        result.Should().Be("?Name=Test&Value=123");
    }

    [TestMethod]
    public void BuildUrlWithQueryString_ShouldReturnEmptyQueryString_WhenDtoHasNoProperties()
    {
        // Arrange
        var dto = new { };

        // Act
        var result = ServiceClientBase.BuildUrlWithQueryString(dto);

        // Assert
        result.Should().Be("?");
    }

    [TestMethod]
    public void BuildUrlWithQueryString_ShouldEscapeSpecialCharacters()
    {
        // Arrange
        var dto = new { Name = "Test & Value", Value = "123/456" };

        // Act
        var result = ServiceClientBase.BuildUrlWithQueryString(dto);

        // Assert
        result.Should().Be("?Name=Test%20%26%20Value&Value=123%2F456");
    }
}
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiGateway.Api.Extensions;

namespace WebApiGateway.UnitTests.Api.Extensions;

[TestClass]
public class HttpContentHeaderExtensionsTests
{
    [TestMethod]
    public void GetContentType_ReturnsCorrectHeader_WhenHeaderExists()
    {
        // Arrange
        var httpResponseMessage = new HttpResponseMessage
        {
            Content =
            {
                Headers =
                {
                    { "Content-Type", "application/json; charset=utf-8" }
                }
            }
        };

        // Act
        var result = httpResponseMessage.Content.Headers.GetContentType();

        // Assert
        result.Should().Be("application/json");
    }

    [TestMethod]
    public void GetContentType_ReturnsDefault_WhenHeaderDoesExist()
    {
        // Arrange
        var httpResponseMessage = new HttpResponseMessage();

        // Act
        var result = httpResponseMessage.Content.Headers.GetContentType();

        // Assert
        result.Should().Be(string.Empty);
    }
}
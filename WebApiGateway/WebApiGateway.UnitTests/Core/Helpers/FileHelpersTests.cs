using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiGateway.Core.Helpers;

namespace WebApiGateway.UnitTests.Core.Helpers;

[TestClass]
public class FileHelpersTests
{
    private const int TruncationLength = 100;

    [TestMethod]
    public void GetTruncatedFileName_ReturnsCorrectFileName_WhenFileNameLengthExceedsTruncationLength()
    {
        // Arrange
        var filename = string.Concat(Enumerable.Repeat("a", 110));

        // Act
        var result = FileHelpers.GetTruncatedFileName(filename, TruncationLength);

        // Assert
        var expectedFilename = string.Concat(Enumerable.Repeat("a", 100));
        result.Should().Be(expectedFilename);
    }

    [TestMethod]
    public void GetTruncatedFileName_ReturnsCorrectFileName_WhenFileNameLengthIsSameAsTruncationLength()
    {
        // Arrange
        var filename = string.Concat(Enumerable.Repeat("a", 100));

        // Act
        var result = FileHelpers.GetTruncatedFileName(filename, TruncationLength);

        // Assert
        result.Should().Be(filename);
    }

    [TestMethod]
    public void GetTruncatedFileName_ReturnsCorrectFileName_WhenFileNameLengthIsLessThanTruncationLength()
    {
        // Arrange
        var filename = string.Concat(Enumerable.Repeat("a", 50));

        // Act
        var result = FileHelpers.GetTruncatedFileName(filename, TruncationLength);

        // Assert
        result.Should().Be(filename);
    }
}
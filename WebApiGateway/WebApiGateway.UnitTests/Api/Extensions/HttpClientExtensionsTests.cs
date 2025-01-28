using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiGateway.Api.Extensions;

namespace WebApiGateway.UnitTests.Api.Extensions;

[TestClass]
public class HttpClientExtensionsTests
{
    private HttpClient _httpClient;

    [TestInitialize]
    public void TestInitialize()
    {
        _httpClient = new HttpClient();
    }

    [TestMethod]
    public void AddIfNotExists_AddsHeaderToDefaultRequestHeaders_IfAHeaderWithAnIdenticalKeyDoesNotExist()
    {
        // Arrange
        const string Key = "headerKey";
        const string Value = "headerValue";

        // Act
        _httpClient.DefaultRequestHeaders.AddIfNotExists(Key, Value);

        // Assert
        _httpClient.DefaultRequestHeaders
            .Should()
            .HaveCount(1)
            .And
            .ContainKey(Key)
            .WhoseValue
            .Should()
            .BeEquivalentTo(Value);
    }

    [TestMethod]
    public void AddIfNotExists_DoesNotHeaderToDefaultRequestHeaders_IfAHeaderWithAnIdenticalKeyDoesExists()
    {
        // Arrange
        const string Key = "headerKey";
        const string OriginalValue = "headerValue";
        const string UpdatedValue = "headerValue";
        _httpClient.DefaultRequestHeaders.Add(Key, OriginalValue);

        // Act
        _httpClient.DefaultRequestHeaders.AddIfNotExists(Key, UpdatedValue);

        // Assert
        _httpClient.DefaultRequestHeaders
            .Should()
            .HaveCount(1)
            .And
            .ContainKey(Key)
            .WhoseValue
            .Should()
            .BeEquivalentTo(OriginalValue);
    }
}
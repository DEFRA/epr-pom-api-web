using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Models.ProducerDetails;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class ProducerDetailsServiceTests
{
    private const int OrganisationId = 1234;
    private Mock<IProducerDetailsClient> _producerDetailsClientMock;
    private Mock<ILogger<ProducerDetailsService>> _loggerMock;
    private ProducerDetailsService _service;

    [TestInitialize]
    public void SetUp()
    {
        _producerDetailsClientMock = new Mock<IProducerDetailsClient>();
        _loggerMock = new Mock<ILogger<ProducerDetailsService>>();
        _service = new ProducerDetailsService(_producerDetailsClientMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetProducerDetails_ShouldLogDebugMessage()
    {
        // Arrange
        _producerDetailsClientMock
            .Setup(client => client.GetProducerDetails(It.IsAny<int>()))
            .ReturnsAsync((GetProducerDetailsResponse)null);

        // Act
        await _service.GetProducerDetails(OrganisationId);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Get Producer Details For Organisation Id {OrganisationId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task GetProducerDetails_ShouldCallProducerDetailsClientWithCorrectOrganisationId()
    {
        // Arrange
        var expectedResponse = new GetProducerDetailsResponse();
        _producerDetailsClientMock
            .Setup(client => client.GetProducerDetails(OrganisationId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.GetProducerDetails(OrganisationId);

        // Assert
        result.Should().Be(expectedResponse);
        _producerDetailsClientMock.Verify(client => client.GetProducerDetails(OrganisationId), Times.Once);
    }

    [TestMethod]
    public async Task GetProducerDetails_ShouldReturnNull_WhenClientReturnsNull()
    {
        // Arrange
        _producerDetailsClientMock
            .Setup(client => client.GetProducerDetails(OrganisationId))
            .ReturnsAsync((GetProducerDetailsResponse)null);

        // Act
        var result = await _service.GetProducerDetails(OrganisationId);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetProducerDetails_ShouldThrowException_WhenClientThrowsException()
    {
        // Arrange
        var expectedException = new HttpRequestException("An error occurred while fetching producer details.");

        _producerDetailsClientMock
            .Setup(client => client.GetProducerDetails(OrganisationId))
            .ThrowsAsync(expectedException);

        // Act
        Func<Task> act = async () => await _service.GetProducerDetails(OrganisationId);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>().WithMessage(expectedException.Message);
    }
}
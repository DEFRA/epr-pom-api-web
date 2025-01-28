using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Controllers;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.ProducerDetails;

namespace WebApiGateway.UnitTests.Api.Controllers
{
    [TestClass]
    public class ProducerDetailsControllerTests
    {
        private Mock<IProducerDetailsService> _producerDetailsServiceMock;
        private Mock<ILogger<ProducerDetailsController>> _loggerMock;
        private Mock<IConfiguration> _configurationMock;
        private ProducerDetailsController _controller;
        private Fixture _fixture;

        [TestInitialize]
        public void Setup()
        {
            _fixture = new Fixture();
            _producerDetailsServiceMock = new Mock<IProducerDetailsService>();
            _loggerMock = new Mock<ILogger<ProducerDetailsController>>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c["LogPrefix"]).Returns("TestPrefix");

            _controller = new ProducerDetailsController(
                _producerDetailsServiceMock.Object,
                _loggerMock.Object,
                _configurationMock.Object);
        }

        [TestMethod]
        public async Task GetProducerDetails_ShouldReturnOk_WhenDataExists()
        {
            // Arrange
            var organisationId = _fixture.Create<int>();
            var producerDetailsResponse = _fixture.Create<GetProducerDetailsResponse>();
            _producerDetailsServiceMock
                .Setup(service => service.GetProducerDetails(organisationId))
                .ReturnsAsync(producerDetailsResponse);

            // Act
            var result = await _controller.GetProducerDetails(organisationId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(producerDetailsResponse);
        }

        [TestMethod]
        public async Task GetProducerDetails_ShouldReturnNoContent_WhenNoData()
        {
            // Arrange
            var organisationId = _fixture.Create<int>();
            _producerDetailsServiceMock
                .Setup(service => service.GetProducerDetails(organisationId))
                .ReturnsAsync((GetProducerDetailsResponse)null);

            // Act
            var result = await _controller.GetProducerDetails(organisationId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeNull();
        }

        [TestMethod]
        public async Task GetProducerDetails_ShouldLogInformation_WhenCalled()
        {
            // Arrange
            var organisationId = _fixture.Create<int>();
            _producerDetailsServiceMock
                .Setup(service => service.GetProducerDetails(organisationId))
                .ReturnsAsync(new GetProducerDetailsResponse());

            // Act
            await _controller.GetProducerDetails(organisationId);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GetProducerDetails")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
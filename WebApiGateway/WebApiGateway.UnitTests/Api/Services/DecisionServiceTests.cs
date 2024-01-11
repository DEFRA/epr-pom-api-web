namespace WebApiGateway.UnitTests.Api.Services;

using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Common.Logging.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Models.Decision;
using WebApiGateway.Core.Options;

[TestClass]
public class DecisionServiceTests
{
    private const string _registrationContainerName = "registration-container-name";
    private const string _pomContainerName = "pom-container-name";

    private static readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    private Mock<IDecisionClient> _decisionStatusClientMock;
    private Mock<ILoggingService> _loggingServiceMock;
    private Mock<ILogger<DecisionService>> _loggerMock;

    private DecisionService _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _decisionStatusClientMock = new Mock<IDecisionClient>();
        var options = Options.Create(
            new StorageAccountOptions
            {
                RegistrationContainer = _registrationContainerName,
                PomContainer = _pomContainerName
            });
        _loggingServiceMock = new Mock<ILoggingService>();
        _loggerMock = new Mock<ILogger<DecisionService>>();
        _systemUnderTest = new DecisionService(_decisionStatusClientMock.Object);
    }

    [TestMethod]
    public async Task GetDecisionsAsync_ReturnsDecisions()
    {
        // Arrange
        const string QueryString = "?key=value";
        var expectedGuid = Guid.NewGuid();
        var decision = new PomDecision { SubmissionId = expectedGuid };
        _decisionStatusClientMock.Setup(x => x.GetDecisionAsync(QueryString)).ReturnsAsync(decision);

        // Act
        var result = await _systemUnderTest.GetDecisionAsync(QueryString);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PomDecision>();
        result.SubmissionId.Should().Be(expectedGuid);
    }
}
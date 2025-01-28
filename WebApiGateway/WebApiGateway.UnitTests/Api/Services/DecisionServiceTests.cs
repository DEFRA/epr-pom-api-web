using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Models.Decision;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class DecisionServiceTests
{
    private Mock<IDecisionClient> _decisionStatusClientMock;

    private DecisionService _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _decisionStatusClientMock = new Mock<IDecisionClient>();
        _systemUnderTest = new DecisionService(_decisionStatusClientMock.Object);
    }

    [TestMethod]
    public async Task GetDecisionsAsync_ReturnsDecisions()
    {
        // Arrange
        const string QueryString = "?key=value";
        var expectedGuid = Guid.NewGuid();
        var decision = new RegulatorDecision { SubmissionId = expectedGuid };
        _decisionStatusClientMock.Setup(x => x.GetDecisionAsync(QueryString)).ReturnsAsync(decision);

        // Act
        var result = await _systemUnderTest.GetDecisionAsync(QueryString);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<RegulatorDecision>();
        result.SubmissionId.Should().Be(expectedGuid);
    }
}
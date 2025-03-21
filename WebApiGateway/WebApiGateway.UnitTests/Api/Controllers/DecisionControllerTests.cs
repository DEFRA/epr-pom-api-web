﻿using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Controllers;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Decision;

namespace WebApiGateway.UnitTests.Api.Controllers;

[TestClass]
public class DecisionControllerTests
{
    private Mock<IDecisionService> _decisionServiceMock;
    private DecisionController _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _decisionServiceMock = new Mock<IDecisionService>();
        _systemUnderTest = new DecisionController(_decisionServiceMock.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
    }

    [TestMethod]
    public async Task GetDecisions_ReturnsOkObjectResult()
    {
        // Arrange
        const string QueryString = "?key=value";
        _systemUnderTest.HttpContext.Request.QueryString = new QueryString(QueryString);
        var decision = new RegulatorDecision();

        _decisionServiceMock.Setup(x => x.GetDecisionAsync(QueryString)).ReturnsAsync(decision);

        // Act
        var result = await _systemUnderTest.GetDecision() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(decision);
        _decisionServiceMock.Verify(x => x.GetDecisionAsync(QueryString), Times.Once);
    }
}
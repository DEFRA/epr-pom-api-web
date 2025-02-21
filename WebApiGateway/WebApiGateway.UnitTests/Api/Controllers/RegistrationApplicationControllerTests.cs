using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Controllers;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.RegistrationFeeCalculation;
using WebApiGateway.Core.Models.Submission;

namespace WebApiGateway.UnitTests.Api.Controllers;

[TestClass]
public class RegistrationApplicationControllerTests
{
    private Mock<IRegistrationApplicationService> _registrationApplicationService;
    private RegistrationApplicationController _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _registrationApplicationService = new Mock<IRegistrationApplicationService>();
        _systemUnderTest = new RegistrationApplicationController(_registrationApplicationService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ReturnsOkObjectResult()
    {
        // Arrange
        var response = new RegistrationApplicationDetails
        {
            RegistrationFeeCalculationDetails = [new RegistrationFeeCalculationDetails()]
        };

        _registrationApplicationService.Setup(x => x.GetRegistrationApplicationDetails(It.IsAny<string>())).ReturnsAsync(response);

        // Act
        var result = await _systemUnderTest.GetRegistrationApplicationDetails() as OkObjectResult;

        // Assert
        result!.Value.Should().BeEquivalentTo(response);
        response.RegistrationFeeCalculationDetails.Should().NotBeNull();

        _registrationApplicationService.Verify(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_ReturnsNoContentResult()
    {
        // Arrange

        // Act
        var result = await _systemUnderTest.GetRegistrationApplicationDetails() as OkObjectResult;

        // Assert
        result!.Should().Be(null);

        _registrationApplicationService.Verify(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task GetRegistrationApplicationDetails_When_GetComplianceSchemeDetails_is_Empty_ReturnsOkObjectResult()
    {
        // Arrange

        // Act
        var result = await _systemUnderTest.GetRegistrationApplicationDetails() as OkObjectResult;

        // Assert
        result.Should().Be(null);

        _registrationApplicationService.Verify(x => x.GetRegistrationApplicationDetails(It.IsAny<string>()), Times.Once);
    }
}
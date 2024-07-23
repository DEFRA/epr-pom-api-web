using AutoFixture.MSTest;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Controllers;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.UnitTests.Api.Controllers
{
    [TestClass]
    public class PrnControllerTests
    {
        private Mock<ILogger<PrnController>> _loggerMock;
        private Mock<IPrnService> _prnService;
        private PrnController _systemUnderTest;

        [TestInitialize]
        public void TestInitialize()
        {
            _loggerMock = new Mock<ILogger<PrnController>>();
            _prnService = new Mock<IPrnService>();
            _systemUnderTest = new PrnController(_prnService.Object, _loggerMock.Object);
        }

        [TestMethod]
        [AutoData]
        public async Task GetAllPrnsForOrganisation_ReturnListOfPrns(Guid orgId, List<PrnModel> response)
        {
            _prnService.Setup(x => x.GetAllPrnsForOrganisation(orgId)).ReturnsAsync(response);
            var result = await _systemUnderTest.GetAllPrnsForOrganisation(orgId);
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(response);
        }

        [TestMethod]
        [AutoData]
        public async Task GetPrnsById_ReturnPrn(Guid id, PrnModel response)
        {
            _prnService.Setup(x => x.GetPrnById(id)).ReturnsAsync(response);
            var result = await _systemUnderTest.GetPrnById(id);
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(response);
        }

        [TestMethod]
        [AutoData]
        public async Task UpdatePrnStatusToAccepted_CallService(Guid id)
        {
            _prnService.Setup(x => x.UpdatePrnStatusToAccepted(id)).Returns(Task.CompletedTask);
            var result = await _systemUnderTest.UpdatePrnStatusToAccepted(id);
            result.Should().BeOfType<NoContentResult>();
            _prnService.Verify();
        }
    }
}

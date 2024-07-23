using AutoFixture.MSTest;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.UnitTests.Api.Services
{
    [TestClass]
    public class PrnServiceTests
    {
        private Mock<ILogger<PrnService>> _loggerMock;
        private Mock<IPrnServiceClient> _prnServiceClient;
        private PrnService _systemUnderTest;

        [TestInitialize]
        public void TestInitialize()
        {
            _loggerMock = new Mock<ILogger<PrnService>>();
            _prnServiceClient = new Mock<IPrnServiceClient>();
            _systemUnderTest = new PrnService(_prnServiceClient.Object, _loggerMock.Object);
        }

        [TestMethod]
        [AutoData]
        public async Task GetAllPrnsForOrganisation_ReturnListOfPrns(Guid orgId, List<PrnModel> response)
        {
            _prnServiceClient.Setup(x => x.GetAllPrnsForOrganisation(orgId)).ReturnsAsync(response);
            var result = await _systemUnderTest.GetAllPrnsForOrganisation(orgId);
            result.Should().BeEquivalentTo(response);
        }

        [TestMethod]
        [AutoData]
        public async Task GetPrnsById_ReturnPrn(Guid id, PrnModel response)
        {
            _prnServiceClient.Setup(x => x.GetPrnById(id)).ReturnsAsync(response);
            var result = await _systemUnderTest.GetPrnById(id);
            result.Should().BeEquivalentTo(response);
        }

        [TestMethod]
        [AutoData]
        public async Task UpdatePrnStatusToAccepted_CallService(Guid id)
        {
            _prnServiceClient.Setup(x => x.UpdatePrnStatusToAccepted(id)).Returns(Task.CompletedTask);
            await _systemUnderTest.UpdatePrnStatusToAccepted(id);
            _prnServiceClient.Verify();
        }
    }
}

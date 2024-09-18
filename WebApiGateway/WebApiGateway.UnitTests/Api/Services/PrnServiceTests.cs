using AutoFixture;
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
        private static readonly IFixture _fixture = new Fixture();
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
        public async Task GetAllPrnsForOrganisation_ReturnListOfPrns()
        {
            var orgId = Guid.NewGuid();
            var response = _fixture.CreateMany<PrnModel>().ToList();
            _prnServiceClient.Setup(x => x.GetAllPrnsForOrganisation()).ReturnsAsync(response);
            var result = await _systemUnderTest.GetAllPrnsForOrganisation();
            result.Should().BeEquivalentTo(response);
        }

        [TestMethod]
        public async Task GetPrnsById_ReturnPrn()
        {
            var id = Guid.NewGuid();
            var response = _fixture.Create<PrnModel>();
            _prnServiceClient.Setup(x => x.GetPrnById(id)).ReturnsAsync(response);
            var result = await _systemUnderTest.GetPrnById(id);
            result.Should().BeEquivalentTo(response);
        }

        [TestMethod]
        public async Task UpdatePrnStatusToAccepted_CallService()
        {
            var updatePrns = _fixture.CreateMany<UpdatePrnStatus>().ToList();
            _prnServiceClient.Setup(x => x.UpdatePrnStatus(updatePrns)).Returns(Task.CompletedTask);
            await _systemUnderTest.UpdatePrnStatus(updatePrns);
            _prnServiceClient.Verify();
        }
    }
}

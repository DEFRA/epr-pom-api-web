using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Models.Pagination;
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

        [TestMethod]
        public async Task GetSearchPrns_ShouldReturnPaginatedResponse_WhenRequestIsValid()
        {
            // Arrange
            var request = _fixture.Create<PaginatedRequest>();
            var expectedResponse = _fixture.Create<PaginatedResponse<PrnModel>>();
            _prnServiceClient
                .Setup(x => x.GetSearchPrns(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _systemUnderTest.GetSearchPrns(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }

        [TestMethod]
        public async Task GetSearchPrns_ShouldThrowException_WhenClientThrowsException()
        {
            // Arrange
            var request = _fixture.Create<PaginatedRequest>();
            var exception = new Exception("Client error");
            _prnServiceClient
                .Setup(x => x.GetSearchPrns(request))
                .ThrowsAsync(exception);

            // Act
            Func<Task> act = async () => await _systemUnderTest.GetSearchPrns(request);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Client error");
        }

        [TestMethod]
        public async Task GetObligationCalculationByOrganisationId_ReturnsCalculations()
        {
            int orgId = 0;
            var calculations = _fixture.CreateMany<ObligationCalculation>().ToList();
            _prnServiceClient.Setup(x => x.GetObligationCalculationByOrganisationIdAsync(orgId)).ReturnsAsync(calculations);
            var result = await _systemUnderTest.GetObligationCalculationsByOrganisationId(orgId);
            result.Should().BeEquivalentTo(calculations);
        }
    }
}
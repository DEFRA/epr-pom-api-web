using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Services;
using WebApiGateway.Core.Models.Pagination;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.UnitTests.Api.Services;

[TestClass]
public class PrnServiceTests
{
    private static readonly IFixture Fixture = new Fixture();
    private Mock<ILogger<PrnService>> _loggerMock;
    private Mock<IPrnServiceClient> _prnServiceClient;
    private Mock<IConfiguration> _configuration;
    private PrnService _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _loggerMock = new Mock<ILogger<PrnService>>();
        _prnServiceClient = new Mock<IPrnServiceClient>();
        _configuration = new Mock<IConfiguration>();
        _systemUnderTest = new PrnService(_prnServiceClient.Object, _loggerMock.Object, _configuration.Object);
    }

    [TestMethod]
    public async Task GetAllPrnsForOrganisation_ReturnListOfPrns()
    {
        var response = Fixture.CreateMany<PrnModel>().ToList();
        _prnServiceClient.Setup(x => x.GetAllPrnsForOrganisation()).ReturnsAsync(response);
        var result = await _systemUnderTest.GetAllPrnsForOrganisation();
        result.Should().BeEquivalentTo(response);
    }

    [TestMethod]
    public async Task GetPrnsById_ReturnPrn()
    {
        var id = Guid.NewGuid();
        var response = Fixture.Create<PrnModel>();
        _prnServiceClient.Setup(x => x.GetPrnById(id)).ReturnsAsync(response);
        var result = await _systemUnderTest.GetPrnById(id);
        result.Should().BeEquivalentTo(response);
    }

    [TestMethod]
    public async Task UpdatePrnStatusToAccepted_CallService()
    {
        var updatePrns = Fixture.CreateMany<UpdatePrnStatus>().ToList();
        _prnServiceClient.Setup(x => x.UpdatePrnStatus(updatePrns)).Returns(Task.CompletedTask);
        await _systemUnderTest.UpdatePrnStatus(updatePrns);
        _prnServiceClient.Verify();
    }

    [TestMethod]
    public async Task GetSearchPrns_ShouldReturnPaginatedResponse_WhenRequestIsValid()
    {
        // Arrange
        var request = Fixture.Create<PaginatedRequest>();
        var expectedResponse = Fixture.Create<PaginatedResponse<PrnModel>>();
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
        var request = Fixture.Create<PaginatedRequest>();
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
    public async Task GetObligationCalculationByYear_ReturnsCalculations()
    {
        int year = DateTime.Now.Year;
        var calculations = Fixture.Create<ObligationModel>();
        _prnServiceClient.Setup(x => x.GetObligationCalculationByYearAsync(year)).ReturnsAsync(calculations);
        var result = await _systemUnderTest.GetObligationCalculationByYear(year);
        result.Should().BeEquivalentTo(calculations);
    }
}
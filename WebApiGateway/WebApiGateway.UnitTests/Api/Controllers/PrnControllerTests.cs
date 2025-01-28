using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Controllers;
using WebApiGateway.Api.Services.Interfaces;
using WebApiGateway.Core.Models.Pagination;
using WebApiGateway.Core.Models.Prns;

namespace WebApiGateway.UnitTests.Api.Controllers;

[TestClass]
public class PrnControllerTests
{
    private static readonly IFixture _fixture = new Fixture();
    private Mock<ILogger<PrnController>> _loggerMock;
    private Mock<IPrnService> _prnService;
    private Mock<IConfiguration> _configuration;
    private PrnController _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _loggerMock = new Mock<ILogger<PrnController>>();
        _prnService = new Mock<IPrnService>();
        _configuration = new Mock<IConfiguration>();
        _systemUnderTest = new PrnController(_prnService.Object, _loggerMock.Object, _configuration.Object);
    }

    [TestMethod]
    public async Task GetAllPrnsForOrganisation_ReturnListOfPrns()
    {
        var response = _fixture.CreateMany<PrnModel>().ToList();
        _prnService.Setup(x => x.GetAllPrnsForOrganisation()).ReturnsAsync(response);
        var result = await _systemUnderTest.GetAllPrnsForOrganisation();
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(response);
    }

    [TestMethod]
    public async Task GetPrnsById_ReturnPrn()
    {
        var id = Guid.NewGuid();
        var response = _fixture.Create<PrnModel>();
        _prnService.Setup(x => x.GetPrnById(id)).ReturnsAsync(response);
        var result = await _systemUnderTest.GetPrnById(id); // Pass complianceSchemeId here
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(response);
    }

    [TestMethod]
    public async Task UpdatePrnStatusToAccepted_CallService()
    {
        var updatePrns = _fixture.CreateMany<UpdatePrnStatus>().ToList();
        _prnService.Setup(x => x.UpdatePrnStatus(updatePrns)).Returns(Task.CompletedTask);
        var result = await _systemUnderTest.UpdatePrnStatusToAccepted(updatePrns);
        result.Should().BeOfType<NoContentResult>();
        _prnService.Verify();
    }

    [TestMethod]
    public async Task GetObligationByYear_ReturnListOfObligations()
    {
        int year = DateTime.Now.Year;
        var response = _fixture.Create<ObligationModel>();
        _prnService.Setup(x => x.GetObligationCalculationByYear(year)).ReturnsAsync(response);
        var result = await _systemUnderTest.GetObligation(year); // Pass complianceSchemeId here
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(response);
    }

    [TestMethod]
    public async Task SearchPrn_ReturnsPaginatedResponse()
    {
        var request = _fixture.Create<PaginatedRequest>();
        var response = _fixture.Create<PaginatedResponse<PrnModel>>();
        _prnService.Setup(x => x.GetSearchPrns(request)).ReturnsAsync(response);
        var result = await _systemUnderTest.SearchPrn(request);
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(response);
    }

    [TestMethod]
    public async Task GetAllPrnsForOrganisation_ReturnsEmptyList_WhenNoPrnsFound()
    {
        _prnService.Setup(x => x.GetAllPrnsForOrganisation()).ReturnsAsync(new List<PrnModel>());
        var result = await _systemUnderTest.GetAllPrnsForOrganisation();
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(new List<PrnModel>());
    }

    [TestMethod]
    public async Task UpdatePrnStatusToAccepted_ReturnsBadRequest_WhenUpdateFails()
    {
        var updatePrns = _fixture.CreateMany<UpdatePrnStatus>().ToList();
        _prnService.Setup(x => x.UpdatePrnStatus(updatePrns)).ThrowsAsync(new Exception("Update failed"));
        Func<Task> act = async () => await _systemUnderTest.UpdatePrnStatusToAccepted(updatePrns);
        await act.Should().ThrowAsync<Exception>().WithMessage("Update failed");
    }
}

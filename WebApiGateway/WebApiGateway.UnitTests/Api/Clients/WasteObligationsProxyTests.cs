using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.ConfigurationExtensions;
using WebApiGateway.Core.Constants;
using WebApiGateway.Core.Models.UserAccount;
using WebApiGateway.Core.Options;
using WebApiGateway.UnitTests.Support;
using WebApiGateway.UnitTests.Support.Extensions;
using WireMock.Server;

namespace WebApiGateway.UnitTests.Api.Clients;

[TestClass]
public class WasteObligationsProxyTests
{
    private WireMockServer WireMock { get; set; }
    private WireMockContext Context { get; set; }
    private ServiceCollection Services { get; set; }
    private Mock<IHttpContextAccessor> MockHttpContextAccessor { get; set; }
    private Mock<IAccountServiceClient> MockAccountServiceClient { get; set; }
    private DefaultHttpContext HttpContext { get; set; }
    private Guid ComplianceSchemeId { get; } = new("af994ed9-2845-4047-9280-d96c4ea8eff2");

    [TestInitialize]
    public void TestInitialize()
    {
        var context = new WireMockContext();
        
        WireMock = context.Server;
        WireMock.Reset();
        Context = context;
        
        var config = new Dictionary<string, string?>
        {
            { $"{WasteObligationsOptions.SectionName}:BaseAddress", context.BaseAddress },
            { $"{WasteObligationsOptions.SectionName}:TokenEndpoint", $"{context.BaseAddress}/token" },
            { $"{WasteObligationsOptions.SectionName}:ClientId", "client_id" },
            { $"{WasteObligationsOptions.SectionName}:ClientSecret", "client_secret" },
            { $"{WasteObligationsOptions.SectionName}:Scope", "scope" },
            { $"{WasteObligationsOptions.SectionName}:TotalRequestTimeout:Timeout", "00:00:40" },
            { $"{WasteObligationsOptions.SectionName}:AttemptTimeout:Timeout", "00:00:05" },
        };

        Services = [];
        Services.AddWasteObligationsProxy();
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(config).Build());

        MockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        MockAccountServiceClient = new Mock<IAccountServiceClient>();
        HttpContext = new DefaultHttpContext();
        MockHttpContextAccessor.Setup(x => x.HttpContext).Returns(HttpContext);
        
        Services.AddTransient(_ => MockHttpContextAccessor.Object);
        Services.AddTransient(_ => MockAccountServiceClient.Object);
    }
    
    [TestMethod]
    public async Task RequiredService_ShouldNotBeNull()
    {
        await using var sp = Services.BuildServiceProvider();

        var service = sp.GetService<IWasteObligationsProxy>();

        service.Should().NotBeNull();
    }
    
    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task GetComplianceDeclarations_WhenUsingHeader_ShouldReturnData(bool toString)
    {
        HttpContext.Items.Add(
            ComplianceScheme.ComplianceSchemeId,
            toString ? ComplianceSchemeId.ToString("D") : ComplianceSchemeId);
        await using var sp = Services.BuildServiceProvider();

        var service = sp.GetRequiredService<IWasteObligationsProxy>();
        const int ObligationYear = 2026;
        const string AccessToken = "access_token";

        WireMock.StubTokenRequest(accessToken: AccessToken);
        WireMock.StubWasteObligationsComplianceDeclarationsRequest(ComplianceSchemeId, ObligationYear, AccessToken);

        var result = await service.GetComplianceDeclarations(ObligationYear, CancellationToken.None);

        result.Should().NotBeNull();
    }
    
    [TestMethod]
    public async Task GetComplianceDeclarations_WhenComplianceDeclarationsNotFound_ShouldReturnNull()
    {
        HttpContext.Items.Add(ComplianceScheme.ComplianceSchemeId, ComplianceSchemeId);
        await using var sp = Services.BuildServiceProvider();

        var service = sp.GetRequiredService<IWasteObligationsProxy>();
        const int ObligationYear = 2026;
        const string AccessToken = "access_token";

        WireMock.StubTokenRequest(accessToken: AccessToken);

        var result = await service.GetComplianceDeclarations(ObligationYear, CancellationToken.None);

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task GetComplianceDeclarations_WhenFallingBackToAccountService_ShouldReturnData()
    {
        HttpContext.Items.Add(ComplianceScheme.ComplianceSchemeId, string.Empty);
        var userId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        HttpContext.User =
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimConstants.ObjectId, userId.ToString("D"))]));
        MockAccountServiceClient.Setup(x => x.GetUserAccount(userId)).ReturnsAsync(new UserAccount
        {
            User = new UserDetails
            {
                Organisations =
                [
                    new OrganisationDetail
                    {
                        Id = organisationId
                    }
                ]
            }
        });
        await using var sp = Services.BuildServiceProvider();

        var service = sp.GetRequiredService<IWasteObligationsProxy>();
        const int ObligationYear = 2026;
        const string AccessToken = "access_token";

        WireMock.StubTokenRequest(accessToken: AccessToken);
        WireMock.StubWasteObligationsComplianceDeclarationsRequest(organisationId, ObligationYear, AccessToken);

        var result = await service.GetComplianceDeclarations(ObligationYear, CancellationToken.None);

        result.Should().NotBeNull();
    }
}
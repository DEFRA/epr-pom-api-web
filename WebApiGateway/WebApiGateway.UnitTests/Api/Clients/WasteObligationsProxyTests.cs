using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Clients;
using WebApiGateway.Api.Clients.Interfaces;
using WebApiGateway.Api.Handlers;
using WebApiGateway.Core.Constants;

namespace WebApiGateway.UnitTests.Api.Clients;

[TestClass]
public class WasteObligationsProxyTests
{
    [TestMethod]
    public async Task Get_ShouldReplaceOrganisationIdInPath()
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        var complianceSchemeId = new Guid("af994ed9-2845-4047-9280-d96c4ea8eff2");
        httpContext.Items.Add(ComplianceScheme.ComplianceSchemeId, complianceSchemeId);
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        var fixtureHandler = new FixtureHandler();
        
        var services = new ServiceCollection();
        services.AddTransient<WasteObligationsAuthorisationHandler>();
        services.AddTransient(_ => fixtureHandler);
        services.AddTransient(_ => new Mock<IAccountServiceClient>().Object);
        services.AddTransient(_ => mockHttpContextAccessor.Object);
        services
            .AddHttpClient<IWasteObligationsProxy, WasteObligationsProxy>()
            .AddHttpMessageHandler<WasteObligationsAuthorisationHandler>()
            .AddHttpMessageHandler<FixtureHandler>()
            .ConfigureHttpClient(httpClient => httpClient.BaseAddress = new Uri("http://localhost"));

        var sp = services.BuildServiceProvider();
        var proxy = sp.GetRequiredService<IWasteObligationsProxy>();
        
        var act = () => proxy.Get(
            "/organisations/:organisationId/compliance-declarations?obligationYear=2026",
            CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
        fixtureHandler.RequestUri?.AbsolutePath.Should().Contain(complianceSchemeId.ToString("D"));
    }

    private class FixtureHandler : DelegatingHandler
    {
        public Uri? RequestUri { get; private set; }
        
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            
            return base.SendAsync(request, cancellationToken);
        }
    }
}
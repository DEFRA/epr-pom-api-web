using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebApiGateway.Api.Attributes;
using WebApiGateway.Core.Constants;

namespace WebApiGateway.UnitTests.ActionFilters
{
    [TestClass]
    public class ComplianceSchemeIdFilterAttributeTests
    {
        [TestMethod]
        public async Task OnActionExecutionAsync_HeaderDoesNotExist_DoesNotAddToHttpContextItems()
        {
            // Arrange
            var attribute = new ComplianceSchemeIdFilterAttribute();
            var httpContext = new DefaultHttpContext(); // Use DefaultHttpContext
            var routeData = new RouteData(); // Provide a valid RouteData instance
            var actionDescriptor = new ActionDescriptor(); // Provide a valid ActionDescriptor instance
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                controller: null);

            var nextMock = new Mock<ActionExecutionDelegate>();

            // Act
            await attribute.OnActionExecutionAsync(actionExecutingContext, nextMock.Object);

            // Assert
            Assert.IsFalse(httpContext.Items.ContainsKey(ComplianceScheme.ComplianceSchemeId));
        }

        [TestMethod]
        public async Task OnActionExecutionAsync_HeaderExists_AddsToHttpContextItems()
        {
            // Arrange
            var attribute = new ComplianceSchemeIdFilterAttribute();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[ComplianceScheme.ComplianceSchemeId] = "TestSchemeId";
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                controller: null);
            var nextMock = new Mock<ActionExecutionDelegate>();
            nextMock.Setup(next => next()).Returns(Task.FromResult<ActionExecutedContext>(null));

            // Act
            await attribute.OnActionExecutionAsync(actionExecutingContext, nextMock.Object);

            // Assert
            Assert.IsTrue(httpContext.Items.ContainsKey(ComplianceScheme.ComplianceSchemeId));
            Assert.AreEqual("TestSchemeId", httpContext.Items[ComplianceScheme.ComplianceSchemeId]);
        }

        [TestMethod]
        public async Task OnActionExecutionAsync_HeaderExistsButEmpty_DoesNotAddToHttpContextItems()
        {
            // Arrange
            var attribute = new ComplianceSchemeIdFilterAttribute();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[ComplianceScheme.ComplianceSchemeId] = string.Empty; // Simulate empty header
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                controller: null);
            var nextMock = new Mock<ActionExecutionDelegate>();
            nextMock.Setup(next => next()).Returns(Task.FromResult<ActionExecutedContext>(null));

            // Act
            await attribute.OnActionExecutionAsync(actionExecutingContext, nextMock.Object);

            // Assert
            Assert.IsFalse(httpContext.Items.ContainsKey(ComplianceScheme.ComplianceSchemeId));
        }

        [TestMethod]
        public async Task OnActionExecutionAsync_InvokesNextDelegate()
        {
            // Arrange
            var attribute = new ComplianceSchemeIdFilterAttribute();
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                controller: null);
            var nextMock = new Mock<ActionExecutionDelegate>();
            nextMock.Setup(next => next()).Returns(Task.FromResult<ActionExecutedContext>(null)).Verifiable();

            // Act
            await attribute.OnActionExecutionAsync(actionExecutingContext, nextMock.Object);

            // Assert
            nextMock.Verify(next => next(), Times.Once);
        }
    }
}
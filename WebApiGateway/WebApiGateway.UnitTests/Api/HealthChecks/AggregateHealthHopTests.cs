using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiGateway.Api.HealthChecks;

namespace WebApiGateway.UnitTests.Api.HealthChecks;

[TestClass]
public class AggregateHealthHopTests
{
    [TestMethod]
    public void TryRead_WhenHeaderIsMissing_StartsAtZero()
    {
        var request = new DefaultHttpContext().Request;

        var isValid = AggregateHealthHop.TryRead(request, 2, out var hop);

        isValid.Should().BeTrue();
        hop.Should().Be(0);
    }

    [DataTestMethod]
    [DataRow("0", 0)]
    [DataRow("2", 2)]
    public void TryRead_WhenHeaderIsValid_ReturnsTheCurrentHop(string headerValue, int expectedHop)
    {
        var request = new DefaultHttpContext().Request;
        request.Headers[AggregateHealthHop.HeaderName] = headerValue;

        var isValid = AggregateHealthHop.TryRead(request, 2, out var hop);

        isValid.Should().BeTrue();
        hop.Should().Be(expectedHop);
    }

    [DataTestMethod]
    [DataRow("-1")]
    [DataRow("3")]
    [DataRow("invalid")]
    [DataRow("999999999999999999999")]
    public void TryRead_WhenHeaderIsInvalid_ReturnsFalse(string headerValue)
    {
        var request = new DefaultHttpContext().Request;
        request.Headers[AggregateHealthHop.HeaderName] = headerValue;

        var isValid = AggregateHealthHop.TryRead(request, 2, out _);

        isValid.Should().BeFalse();
    }

    [TestMethod]
    public void TryRead_WhenHeaderIsRepeated_ReturnsFalse()
    {
        var request = new DefaultHttpContext().Request;
        request.Headers[AggregateHealthHop.HeaderName] = new StringValues(["1", "2"]);

        var isValid = AggregateHealthHop.TryRead(request, 2, out _);

        isValid.Should().BeFalse();
    }
}

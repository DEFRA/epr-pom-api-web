using System.Net;
using AnyOfTypes;
using WireMock.Matchers;
using WireMock.Models;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace WebApiGateway.UnitTests.Support.Extensions;

public static class OAuth2Extensions
{
    public const string AccessToken = "access_token";

    public static void StubTokenRequest(
        this WireMockServer wireMock,
        string accessToken = AccessToken,
        int expiryInSeconds = 3600,
        string? scope = "scope")
    {
        AnyOf<string, StringPattern>[] patterns =
        [
            "grant_type=client_credentials",
            "client_id=client_id",
            "client_secret=client_secret",
        ];

        if (scope is not null)
        {
            patterns = patterns.Append($"scope={scope}").ToArray();
        }

        wireMock
            .Given(
                Request
                    .Create()
                    .UsingPost()
                    .WithPath("/token")
                    .WithHeader("Content-Type", "application/x-www-form-urlencoded")
                    .WithBody(new FormUrlEncodedMatcher(patterns, matchOperator: MatchOperator.And)))
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBodyAsJson(new { access_token = accessToken, expires_in = expiryInSeconds }));
    }
}
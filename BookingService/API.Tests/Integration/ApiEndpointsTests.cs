using System.Net;
using System.Net.Http.Json;
using API.Dto.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace API.Tests.Integration;

[TestFixture]
[NonParallelizable]
public class ApiEndpointsTests
{
    private ApiWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new ApiWebApplicationFactory();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Authenticate_ReturnsTokenForValidCredentials()
    {
        var response = await _client.PostAsJsonAsync("/api/auth", new AuthRequest("alice", "password"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.That(body?.Token, Is.EqualTo("test-token"));
    }

    [Test]
    public async Task Authenticate_ReturnsProblemDetailsForInvalidCredentials()
    {
        var response = await _client.PostAsJsonAsync("/api/auth", new AuthRequest("", ""));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var body = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        Assert.That(body?.Title, Is.EqualTo("Bad Request"));
        Assert.That(body?.Status, Is.EqualTo(400));
    }

    [Test]
    public async Task ProtectedResourceEndpoint_RejectsAnonymousRequest()
    {
        var response = await _client.GetAsync("/api/resources");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    private sealed record ProblemDetailsResponse(string? Type, string? Title, int? Status, string? Detail, string? Instance);
}

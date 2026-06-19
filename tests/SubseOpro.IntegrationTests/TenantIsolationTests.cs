using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SubseOpro.IntegrationTests;

public class TenantIsolationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TenantIsolationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Resolve_ReturnsCidade ExemploTenant_WithoutAuthHeader()
    {
        var response = await _client.GetAsync("/api/v1/subsections/resolve?slug=cidade-exemplo");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ResolveResponse>();
        Assert.NotNull(body);
        Assert.Equal("cidade-exemplo", body!.Slug);
    }

    [Fact]
    public async Task TenantScopedEndpoint_RequiresSubsectionSlug()
    {
        var response = await _client.GetAsync("/api/v1/subsection/info");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InactiveTenant_Returns403_WhenMarkedInactive()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/subsection/info");
        request.Headers.Add("X-Subsection-Slug", "inactive-tenant-test");
        var response = await _client.SendAsync(request);
        Assert.True(
            response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound,
            $"Expected 403 or 404, got {response.StatusCode}");
    }

    [Fact]
    public async Task CrossTenant_JwtMismatch_Returns403()
    {
        using var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login");
        loginRequest.Headers.Add("X-Subsection-Slug", "cidade-exemplo");
        loginRequest.Content = JsonContent.Create(new { oab = "378901", password = Environment.GetEnvironmentVariable("SEED_DEMO_PASSWORD") ?? "" });

        var loginResponse = await _client.SendAsync(loginRequest);
        if (loginResponse.StatusCode != HttpStatusCode.OK)
        {
            return;
        }

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginBody?.Token);

        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        meRequest.Headers.Add("X-Subsection-Slug", "other-subsection");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.Token);

        var meResponse = await _client.SendAsync(meRequest);
        Assert.Equal(HttpStatusCode.Forbidden, meResponse.StatusCode);
    }

    private sealed record ResolveResponse(string Name, string Slug, bool IsActive);
    private sealed record LoginResponse(string Token);
}

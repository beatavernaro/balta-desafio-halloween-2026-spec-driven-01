using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator.Api.Data;

namespace PasswordGenerator.IntegrationTests;

public sealed class PasswordRetrievalTests : IDisposable
{
    private readonly PasswordApiFactory _factory = new();
    private readonly HttpClient _client;

    public PasswordRetrievalTests() => _client = _factory.CreateClient();

    [Fact]
    public async Task GetPassword_ExistingIdentifier_ReturnsOkWithOriginalPassword()
    {
        var created = await CreatePasswordAsync();

        var response = await _client.GetAsync($"/passwords/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PasswordResponseBody>();
        Assert.NotNull(body);
        Assert.Equal(created.Id, body.Id);
        Assert.Equal(created.Password, body.Password);
    }

    [Fact]
    public async Task GetPassword_ExistingIdentifier_DoesNotModifyStoredPassword()
    {
        var created = await CreatePasswordAsync();

        var response = await _client.GetAsync($"/passwords/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PasswordDbContext>();
        var storedPassword = await dbContext.GeneratedPasswords.FindAsync(created.Id);

        Assert.NotNull(storedPassword);
        Assert.Equal(created.Password, storedPassword.Value);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<PasswordResponseBody> CreatePasswordAsync()
    {
        var response = await _client.PostAsync("/passwords", content: null);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PasswordResponseBody>();
        Assert.NotNull(body);

        return body;
    }

    private sealed record PasswordResponseBody(Guid Id, string Password);
}

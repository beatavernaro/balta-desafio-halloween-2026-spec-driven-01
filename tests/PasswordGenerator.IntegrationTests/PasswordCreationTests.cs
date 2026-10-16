using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator.Api.Data;

namespace PasswordGenerator.IntegrationTests;

public sealed class PasswordCreationTests : IDisposable
{
    private readonly PasswordApiFactory _factory = new();
    private readonly HttpClient _client;

    public PasswordCreationTests() => _client = _factory.CreateClient();

    [Fact]
    public async Task CreatePassword_ReturnsCreatedWithIdentifierAndValidPassword()
    {
        var response = await _client.PostAsync("/passwords", content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PasswordResponseBody>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.True(Api.Services.PasswordGenerator.IsValid(body.Password));
    }

    [Fact]
    public async Task CreatePassword_PersistsReturnedPassword()
    {
        var response = await _client.PostAsync("/passwords", content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PasswordResponseBody>();
        Assert.NotNull(body);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PasswordDbContext>();
        var storedPassword = await dbContext.GeneratedPasswords.FindAsync(body.Id);

        Assert.NotNull(storedPassword);
        Assert.Equal(body.Password, storedPassword.Value);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private sealed record PasswordResponseBody(Guid Id, string Password);
}

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
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

    // CB-12
    [Fact]
    public async Task CreatePassword_WhenStorageFails_ReturnsProblemDetailsWithoutConfirmingSuccess()
    {
        using var failingFactory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.ConfigureDbContext<PasswordDbContext>(options =>
                    options.AddInterceptors(new FailingSaveChangesInterceptor()))));
        using var client = failingFactory.CreateClient();

        var response = await client.PostAsync("/passwords", content: null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(FailingSaveChangesInterceptor.FailureMessage, body);
        Assert.DoesNotContain("Exception", body);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsBody>();
        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal("Internal server error", problem.Title);
        Assert.Equal("The operation could not be completed.", problem.Detail);

        using var scope = failingFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PasswordDbContext>();
        Assert.Empty(dbContext.GeneratedPasswords);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private sealed record PasswordResponseBody(Guid Id, string Password);

    private sealed record ProblemDetailsBody(string Type, string Title, int Status, string Detail);
}

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PasswordGenerator.Api.Data;

namespace PasswordGenerator.IntegrationTests;

public sealed class PasswordCreationTests : IDisposable
{
    private const int MultipleCreationCount = 20;

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
        using var failingFactory = CustomizeFactory(storageFailure: new FailingSaveChangesInterceptor());
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

    // CB-13
    [Fact]
    public async Task CreatePassword_MultipleRequests_AssignUniqueIdentifiers()
    {
        var identifiers = new List<Guid>();

        for (var request = 0; request < MultipleCreationCount; request++)
        {
            var response = await _client.PostAsync("/passwords", content: null);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<PasswordResponseBody>();
            Assert.NotNull(body);
            identifiers.Add(body.Id);
        }

        Assert.Equal(MultipleCreationCount, identifiers.Distinct().Count());

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PasswordDbContext>();
        var storedIdentifiers = await dbContext.GeneratedPasswords.Select(password => password.Id).ToListAsync();

        Assert.Equal(identifiers.Order(), storedIdentifiers.Order());
    }

    [Fact]
    public async Task CreatePassword_DoesNotLogPasswordValue()
    {
        var logs = new CapturingLoggerProvider();
        using var loggingFactory = CustomizeFactory(logs: logs);
        using var client = loggingFactory.CreateClient();

        var response = await client.PostAsync("/passwords", content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PasswordResponseBody>();
        Assert.NotNull(body);

        // Sanity check: the operation itself is logged, so the capture is working.
        Assert.Contains(logs.Entries, entry => entry.Contains(body.Id.ToString()));
        Assert.DoesNotContain(logs.Entries, entry => entry.Contains(body.Password));
    }

    [Fact]
    public async Task CreatePassword_WhenStorageFails_DoesNotLogPasswordValue()
    {
        var logs = new CapturingLoggerProvider();
        var storageFailure = new FailingSaveChangesInterceptor();
        using var failingFactory = CustomizeFactory(logs, storageFailure);
        using var client = failingFactory.CreateClient();

        var response = await client.PostAsync("/passwords", content: null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        // Sanity checks: a password was generated and the failure itself was logged.
        var attemptedPassword = Assert.Single(storageFailure.AttemptedPasswords);
        Assert.Contains(logs.Entries, entry => entry.Contains(FailingSaveChangesInterceptor.FailureMessage));
        Assert.DoesNotContain(logs.Entries, entry => entry.Contains(attemptedPassword));
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private WebApplicationFactory<Program> CustomizeFactory(
        CapturingLoggerProvider? logs = null,
        FailingSaveChangesInterceptor? storageFailure = null) =>
        _factory.WithWebHostBuilder(builder =>
        {
            if (logs is not null)
            {
                // Capture every level, not only what the default configuration would emit.
                builder.ConfigureLogging(logging => logging
                    .AddProvider(logs)
                    .AddFilter<CapturingLoggerProvider>(category: null, LogLevel.Trace));
            }

            if (storageFailure is not null)
            {
                builder.ConfigureTestServices(services =>
                    services.ConfigureDbContext<PasswordDbContext>(options =>
                        options.AddInterceptors(storageFailure)));
            }
        });

    private sealed record PasswordResponseBody(Guid Id, string Password);

    private sealed record ProblemDetailsBody(string Type, string Title, int Status, string Detail);
}

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

    // CB-10
    [Fact]
    public async Task GetPassword_UnknownIdentifier_ReturnsNotFoundProblemDetails()
    {
        var response = await _client.GetAsync($"/passwords/{Guid.NewGuid()}");

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.NotFound,
            "Password not found",
            "No password was found for the provided identifier.");
    }

    // CB-11
    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("12345")]
    [InlineData("3d34a5f3-a993-4245-b994-3e6da1c7d8d")]
    [InlineData("3d34a5f3-a993-4245-b994-3e6da1c7d8dz")]
    public async Task GetPassword_InvalidIdentifier_ReturnsBadRequestProblemDetails(string identifier)
    {
        var response = await _client.GetAsync($"/passwords/{identifier}");

        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.BadRequest,
            "Invalid identifier",
            "The provided identifier is not a valid GUID.");
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

    private static async Task AssertProblemDetailsAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedTitle,
        string expectedDetail)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsBody>();
        Assert.NotNull(problem);
        Assert.Equal("about:blank", problem.Type);
        Assert.Equal((int)expectedStatus, problem.Status);
        Assert.Equal(expectedTitle, problem.Title);
        Assert.Equal(expectedDetail, problem.Detail);
    }

    private sealed record PasswordResponseBody(Guid Id, string Password);

    private sealed record ProblemDetailsBody(string Type, string Title, int Status, string Detail);
}

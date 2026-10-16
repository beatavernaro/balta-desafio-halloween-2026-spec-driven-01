using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace PasswordGenerator.IntegrationTests;

public sealed class PasswordContractTests : IDisposable
{
    // The OpenAPI document is API documentation (development only), not a functional operation.
    private const string OpenApiDocumentRoute = "/openapi/{documentName}.json";

    private readonly PasswordApiFactory _factory = new();
    private readonly HttpClient _client;

    public PasswordContractTests() => _client = _factory.CreateClient();

    [Fact]
    public async Task CreatePassword_ResponseBody_ContainsOnlyIdAndPassword()
    {
        var response = await _client.PostAsync("/passwords", content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await AssertPasswordContractAsync(response);
    }

    [Fact]
    public async Task GetPassword_ResponseBody_ContainsOnlyIdAndPassword()
    {
        var created = await _client.PostAsync("/passwords", content: null);
        using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = createdBody.RootElement.GetProperty("id").GetString();

        var response = await _client.GetAsync($"/passwords/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertPasswordContractAsync(response);
    }

    [Theory]
    [InlineData("not-a-guid", 400)]
    [InlineData("4f9a2c1e-0b7d-4e8a-9c3f-2d1b0a9e8f7c", 404)]
    public async Task GetPassword_ErrorResponse_FollowsProblemDetailsContract(string identifier, int expectedStatus)
    {
        var response = await _client.GetAsync($"/passwords/{identifier}");

        Assert.Equal(expectedStatus, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = body.RootElement;
        Assert.Equal("about:blank", root.GetProperty("type").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("title").GetString()));
        Assert.Equal(expectedStatus, root.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("detail").GetString()));
    }

    [Fact]
    public void Application_ExposesOnlyTwoFunctionalOperations()
    {
        var operations = _factory.Services
            .GetServices<EndpointDataSource>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText != OpenApiDocumentRoute)
            .SelectMany(endpoint => endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()!.HttpMethods
                .Select(method => $"{method} {endpoint.RoutePattern.RawText}"))
            .Order();

        Assert.Equal(["GET /passwords/{id}", "POST /passwords"], operations);
    }

    [Theory]
    [InlineData("GET", "/passwords")]
    [InlineData("PUT", "/passwords/4f9a2c1e-0b7d-4e8a-9c3f-2d1b0a9e8f7c")]
    [InlineData("DELETE", "/passwords/4f9a2c1e-0b7d-4e8a-9c3f-2d1b0a9e8f7c")]
    public async Task UnsupportedOperation_ReturnsMethodNotAllowed(string method, string path)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static async Task AssertPasswordContractAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = body.RootElement;
        var propertyNames = root.EnumerateObject().Select(property => property.Name).Order();

        Assert.Equal(["id", "password"], propertyNames);
        Assert.True(Guid.TryParseExact(root.GetProperty("id").GetString(), "D", out _));
        Assert.Equal(JsonValueKind.String, root.GetProperty("password").ValueKind);
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using PasswordGenerator.Api.Contracts;
using PasswordGenerator.Api.Services;

namespace PasswordGenerator.Api.Endpoints;

public static class PasswordEndpoints
{
    public static IEndpointRouteBuilder MapPasswordEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/passwords", CreatePasswordAsync);

        return endpoints;
    }

    private static async Task<Created<PasswordResponse>> CreatePasswordAsync(
        PasswordService passwordService,
        CancellationToken cancellationToken)
    {
        var generatedPassword = await passwordService.CreateAsync(cancellationToken);

        return TypedResults.Created(
            $"/passwords/{generatedPassword.Id}",
            new PasswordResponse(generatedPassword.Id, generatedPassword.Value));
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using PasswordGenerator.Api.Contracts;
using PasswordGenerator.Api.Services;

namespace PasswordGenerator.Api.Endpoints;

public static class PasswordEndpoints
{
    public static IEndpointRouteBuilder MapPasswordEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/passwords", CreatePasswordAsync);
        endpoints.MapGet("/passwords/{id}", GetPasswordAsync);

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

    private static async Task<Results<Ok<PasswordResponse>, NotFound>> GetPasswordAsync(
        Guid id,
        PasswordService passwordService,
        CancellationToken cancellationToken)
    {
        var storedPassword = await passwordService.GetByIdAsync(id, cancellationToken);

        if (storedPassword is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new PasswordResponse(storedPassword.Id, storedPassword.Value));
    }
}

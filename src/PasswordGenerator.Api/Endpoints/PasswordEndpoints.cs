using Microsoft.AspNetCore.Http.HttpResults;
using PasswordGenerator.Api.Contracts;
using PasswordGenerator.Api.Services;

namespace PasswordGenerator.Api.Endpoints;

public static class PasswordEndpoints
{
    public static IEndpointRouteBuilder MapPasswordEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/passwords", CreatePasswordAsync);

        endpoints.MapGet("/passwords/{id}", GetPasswordAsync)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

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

    // The identifier is received as text so an invalid format produces the documented 400 ProblemDetails
    // instead of a framework binding failure.
    private static async Task<Results<Ok<PasswordResponse>, ProblemHttpResult>> GetPasswordAsync(
        string id,
        PasswordService passwordService,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var passwordId))
        {
            return TypedResults.Problem(
                type: "about:blank",
                title: "Invalid identifier",
                detail: "The provided identifier is not a valid GUID.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var storedPassword = await passwordService.GetByIdAsync(passwordId, cancellationToken);

        if (storedPassword is null)
        {
            return TypedResults.Problem(
                type: "about:blank",
                title: "Password not found",
                detail: "No password was found for the provided identifier.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return TypedResults.Ok(new PasswordResponse(storedPassword.Id, storedPassword.Value));
    }
}

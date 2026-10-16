using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Data;
using PasswordGenerator.Api.Endpoints;
using PasswordGenerator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PasswordDatabase")
    ?? throw new InvalidOperationException("Connection string 'PasswordDatabase' was not found.");

builder.Services.AddDbContext<PasswordDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<PasswordService>();
builder.Services.AddOpenApi();

var app = builder.Build();

// The middleware logs the exception; the client only receives a generic ProblemDetails, in every environment.
app.UseExceptionHandler(exceptionHandlerApp =>
    exceptionHandlerApp.Run(context =>
        Results.Problem(
            type: "about:blank",
            title: "Internal server error",
            detail: "The operation could not be completed.",
            statusCode: StatusCodes.Status500InternalServerError)
        .ExecuteAsync(context)));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPasswordEndpoints();

app.Run();

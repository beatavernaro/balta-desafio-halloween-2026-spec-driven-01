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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPasswordEndpoints();

app.Run();

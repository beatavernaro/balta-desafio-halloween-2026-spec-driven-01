using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PasswordDatabase")
    ?? throw new InvalidOperationException("Connection string 'PasswordDatabase' was not found.");

builder.Services.AddDbContext<PasswordDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();

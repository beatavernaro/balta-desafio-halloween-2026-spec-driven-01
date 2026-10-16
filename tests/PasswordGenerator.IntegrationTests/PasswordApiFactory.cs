using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PasswordGenerator.Api.Data;

namespace PasswordGenerator.IntegrationTests;

// Each factory owns a private in-memory SQLite database, kept alive by an open connection for the factory's lifetime.
public sealed class PasswordApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _keepAliveConnection =
        new($"Data Source=password-tests-{Guid.NewGuid()};Mode=Memory;Cache=Shared");

    public PasswordApiFactory() => _keepAliveConnection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:PasswordDatabase", _keepAliveConnection.ConnectionString);

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<PasswordDbContext>().Database.Migrate();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _keepAliveConnection.Dispose();
        }
    }
}

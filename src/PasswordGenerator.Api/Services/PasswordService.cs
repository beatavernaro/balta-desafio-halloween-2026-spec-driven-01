using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Data;
using PasswordGenerator.Api.Models;

namespace PasswordGenerator.Api.Services;

public partial class PasswordService(PasswordDbContext dbContext, ILogger<PasswordService> logger)
{
    public async Task<GeneratedPassword> CreateAsync(CancellationToken cancellationToken)
    {
        var generatedPassword = new GeneratedPassword
        {
            Id = Guid.NewGuid(),
            Value = PasswordGenerator.Generate(),
        };

        dbContext.GeneratedPasswords.Add(generatedPassword);

        // A generation is only confirmed after the password has been persisted (RN-07).
        await dbContext.SaveChangesAsync(cancellationToken);

        // Only the identifier is logged: password values must never reach the logs.
        LogPasswordCreated(logger, generatedPassword.Id);

        return generatedPassword;
    }

    // Read-only query: the stored password is returned exactly as persisted (RN-08).
    public Task<GeneratedPassword?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.GeneratedPasswords
            .AsNoTracking()
            .SingleOrDefaultAsync(password => password.Id == id, cancellationToken);

    [LoggerMessage(Level = LogLevel.Information, Message = "Password {PasswordId} generated and stored.")]
    private static partial void LogPasswordCreated(ILogger logger, Guid passwordId);
}

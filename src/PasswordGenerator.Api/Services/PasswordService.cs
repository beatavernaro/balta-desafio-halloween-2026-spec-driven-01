using PasswordGenerator.Api.Data;
using PasswordGenerator.Api.Models;

namespace PasswordGenerator.Api.Services;

public class PasswordService(PasswordDbContext dbContext, ILogger<PasswordService> logger)
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
        logger.LogInformation("Password {PasswordId} generated and stored.", generatedPassword.Id);

        return generatedPassword;
    }
}

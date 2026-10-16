using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PasswordGenerator.IntegrationTests;

// Simulates a storage failure on every save, before anything reaches the database.
public sealed class FailingSaveChangesInterceptor : SaveChangesInterceptor
{
    public const string FailureMessage = "Simulated storage failure.";

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result) =>
        throw new DbUpdateException(FailureMessage);

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default) =>
        throw new DbUpdateException(FailureMessage);
}

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PasswordGenerator.Api.Models;

namespace PasswordGenerator.IntegrationTests;

// Simulates a storage failure on every save, before anything reaches the database.
public sealed class FailingSaveChangesInterceptor : SaveChangesInterceptor
{
    public const string FailureMessage = "Simulated storage failure.";

    private readonly ConcurrentQueue<string> _attemptedPasswords = new();

    // Passwords the application tried to save, so tests can check they never leak after the failure.
    public IReadOnlyCollection<string> AttemptedPasswords => _attemptedPasswords;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result) =>
        Fail(eventData);

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default) =>
        new(Fail(eventData));

    private InterceptionResult<int> Fail(DbContextEventData eventData)
    {
        foreach (var entry in eventData.Context!.ChangeTracker.Entries<GeneratedPassword>())
        {
            _attemptedPasswords.Enqueue(entry.Entity.Value);
        }

        throw new DbUpdateException(FailureMessage);
    }
}

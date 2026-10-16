using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api.Models;

namespace PasswordGenerator.Api.Data;

public class PasswordDbContext(DbContextOptions<PasswordDbContext> options) : DbContext(options)
{
    private const int PasswordLength = 16;

    public DbSet<GeneratedPassword> GeneratedPasswords => Set<GeneratedPassword>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeneratedPassword>(entity =>
        {
            // SQLite does not enforce declared column lengths, so the exact length is also guarded by a check constraint.
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_GeneratedPasswords_Value_Length",
                $"length(\"Value\") = {PasswordLength}"));

            entity.HasKey(password => password.Id);

            entity.Property(password => password.Id)
                .ValueGeneratedNever();

            entity.Property(password => password.Value)
                .IsRequired()
                .HasMaxLength(PasswordLength);
        });
    }
}

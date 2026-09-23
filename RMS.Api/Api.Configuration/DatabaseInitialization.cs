using Microsoft.EntityFrameworkCore;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Infrastructure.Persistence.EF.Core;
using RMS.Shared.Enums;

namespace RMS.Api.Api.Configuration;

/// <summary>
/// پایگاه داده را در زمان راه‌اندازی به آخرین migration ارتقا می‌دهد و کاربر root را ایجاد می‌کند.
/// </summary>
public static class DatabaseInitialization
{
    private const string DefaultRootFirstName = "علی";
    private const string DefaultRootLastName = "روستایی";
    private const string DefaultRootMobileNumber = "09102051231";
    private const string DefaultRootPassword = "123qwe!@#";

    public static async Task InitializeDatabaseAsync(
        this WebApplication app,
        CancellationToken token = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<RmsDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DatabaseInitialization));

        if (db.Database.IsRelational())
            await db.Database.MigrateAsync(token);
        else
            await db.Database.EnsureCreatedAsync(token);

        var rootSection = app.Configuration.GetSection("DatabaseInitialization:RootUser");
        var rootFirstName = GetConfiguredValue(rootSection["FirstName"], DefaultRootFirstName);
        var rootLastName = GetConfiguredValue(rootSection["LastName"], DefaultRootLastName);
        var rootMobileNumber = GetConfiguredValue(rootSection["MobileNumber"], DefaultRootMobileNumber);
        var rootPassword = GetConfiguredValue(rootSection["Password"], DefaultRootPassword);

        if (await db.UserDs.AnyAsync(
                user => user.MobileNumber == rootMobileNumber,
                token))
        {
            return;
        }

        db.UserDs.Add(new UserEntities
        {
            Id = Guid.NewGuid(),
            FirstName = rootFirstName,
            LastName = rootLastName,
            MobileNumber = rootMobileNumber,
            PasswordHash = passwordHasher.Hash(rootPassword),
            Role = UserRole.Administrator
        });

        try
        {
            await db.SaveChangesAsync(token);
            logger.LogWarning(
                "Default root user with mobile number {MobileNumber} was created. Change its password immediately.",
                rootMobileNumber);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            if (!await db.UserDs.AnyAsync(
                    user => user.MobileNumber == rootMobileNumber,
                    token))
            {
                throw;
            }
        }
    }

    private static string GetConfiguredValue(string? value, string defaultValue) =>
        string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
}
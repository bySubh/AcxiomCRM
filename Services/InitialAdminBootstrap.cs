using System.Security.Claims;
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

/// <summary>Explicit, local command-line operation; never exposed as an HTTP endpoint.</summary>
public static class InitialAdminBootstrap
{
    public static async Task<int> RunAsync(IServiceProvider services, IConfiguration configuration)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("ACXIOMCRM_ADMIN_BOOTSTRAP"), "YES", StringComparison.Ordinal))
        {
            Console.Error.WriteLine("Admin bootstrap refused: set ACXIOMCRM_ADMIN_BOOTSTRAP=YES explicitly.");
            return 1;
        }

        var email = Environment.GetEnvironmentVariable("ACXIOMCRM_ADMIN_EMAIL")?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            Console.Error.WriteLine("Admin bootstrap refused: ACXIOMCRM_ADMIN_EMAIL is required.");
            return 1;
        }

        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var audit = scope.ServiceProvider.GetRequiredService<AuditService>();

            // Microsoft.Data.Sqlite begins non-deferred (IMMEDIATE) transactions by default.
            // Acquiring the SQLite writer reservation before checking for an existing Admin
            // serializes competing bootstrap processes on the same database file.
            await using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
            if (!await roles.RoleExistsAsync("Admin") || !await roles.RoleExistsAsync("SalesExecutive"))
                throw new InvalidOperationException("Required roles are missing; run the application once to seed roles.");

            if ((await users.GetUsersInRoleAsync("Admin")).Count != 0)
                throw new InvalidOperationException("An Admin already exists. Bootstrap is forbidden.");

            var user = await users.FindByEmailAsync(email)
                ?? throw new InvalidOperationException("Selected registered account was not found.");
            if (!string.Equals(user.NormalizedEmail, users.NormalizeEmail(email), StringComparison.Ordinal))
                throw new InvalidOperationException("Selected account email does not match.");
            var previousRoles = await users.GetRolesAsync(user);
            if (previousRoles.Count != 1 || previousRoles[0] != "SalesExecutive")
                throw new InvalidOperationException("Selected account must have exactly the SalesExecutive role.");

            EnsureSucceeded(await users.RemoveFromRoleAsync(user, "SalesExecutive"), "Remove SalesExecutive");
            EnsureSucceeded(await users.AddToRoleAsync(user, "Admin"), "Assign Admin");
            EnsureSucceeded(await users.UpdateSecurityStampAsync(user), "Refresh security stamp");

            // The existing audit service uses the same scoped DbContext and therefore
            // participates in the transaction, committing role and audit atomically.
            var actor = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }, "LocalAdminBootstrap"));
            await audit.Write(actor, "InitialAdminBootstrapRoleChange", "User", user.Id,
                new { Role = "SalesExecutive" }, new { Role = "Admin" });
                await transaction.CommitAsync();
                Console.WriteLine("Initial Admin provisioned successfully. Clear the bootstrap environment variables.");
                return 0;
            }
            catch
            {
                // Preserve the original failure even if rollback itself fails.
                try { await transaction.RollbackAsync(); }
                catch { /* The original provisioning failure takes precedence. */ }
                throw;
            }
        }
        catch (Exception)
        {
            // Never print provider exceptions, connection strings, or account details.
            Console.Error.WriteLine("Admin bootstrap failed; no role change was committed. Check restricted application diagnostics.");
            return 1;
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException($"{operation} failed: " +
                string.Join("; ", result.Errors.Select(x => x.Code)));
    }
}

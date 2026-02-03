using FaunaFinder.Identity.Database;
using FaunaFinder.Identity.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Seeder;

public static class IdentitySeeder
{
    private const string AdminEmail = "admin@gmail.com";
    private const string AdminPassword = "Admin@123";
    private const string AdminDisplayName = "Administrator";

    public static async Task SeedAsync(IdentityDbContext context, CancellationToken cancellationToken = default)
    {
        var existingAdmin = await context.Users
            .FirstOrDefaultAsync(u => u.Email == AdminEmail, cancellationToken);

        if (existingAdmin is not null)
            return;

        var user = new User
        {
            UserName = AdminEmail,
            NormalizedUserName = AdminEmail.ToUpperInvariant(),
            Email = AdminEmail,
            NormalizedEmail = AdminEmail.ToUpperInvariant(),
            EmailConfirmed = true,
            DisplayName = AdminDisplayName,
            Role = UserRole.Admin,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, AdminPassword);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}

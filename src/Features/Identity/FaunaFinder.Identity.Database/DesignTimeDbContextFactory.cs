using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FaunaFinder.Identity.Database;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();

        // Use a placeholder connection string for migrations
        optionsBuilder.UseNpgsql("Host=localhost;Database=identity;Username=postgres;Password=postgres", npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("FaunaFinder.Identity.Database");
        });
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new IdentityDbContext(optionsBuilder.Options);
    }
}

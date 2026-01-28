using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FaunaFinder.Wildlife.Database;

/// <summary>
/// Factory for creating WildlifeDbContext at design time for EF Core tools.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WildlifeDbContext>
{
    public WildlifeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WildlifeDbContext>();

        // Use a dummy connection string for migrations
        optionsBuilder.UseNpgsql("Host=localhost;Database=faunafinder_wildlife;Username=postgres;Password=postgres",
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("FaunaFinder.Wildlife.Database");
                npgsqlOptions.UseNetTopologySuite();
            });
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new WildlifeDbContext(optionsBuilder.Options);
    }
}

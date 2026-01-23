using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FaunaFinder.Database;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FaunaFinderContext>
{
    public FaunaFinderContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FaunaFinderContext>();

        // Use a placeholder connection string for migrations
        optionsBuilder.UseNpgsql("Host=localhost;Database=faunafinder;Username=postgres;Password=postgres", npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("FaunaFinder.Database");
            npgsqlOptions.UseNetTopologySuite();
        });
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new FaunaFinderContext(optionsBuilder.Options);
    }
}

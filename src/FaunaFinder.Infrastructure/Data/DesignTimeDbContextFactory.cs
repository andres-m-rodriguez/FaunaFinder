using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FaunaFinder.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FaunaFinderDbContext>
{
    public FaunaFinderDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FaunaFinderDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=faunafinder;Username=postgres;Password=postgres");

        return new FaunaFinderDbContext(optionsBuilder.Options);
    }
}

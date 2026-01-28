using FaunaFinder.Wildlife.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Wildlife.Database;

public sealed class WildlifeDbContext(DbContextOptions<WildlifeDbContext> options)
    : DbContext(options)
{
    public DbSet<Species> Species => Set<Species>();
    public DbSet<SpeciesLocation> SpeciesLocations => Set<SpeciesLocation>();
    public DbSet<Sighting> Sightings => Set<Sighting>();
    public DbSet<UserSpecies> UserSpecies => Set<UserSpecies>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from nested classes
        modelBuilder.ApplyConfiguration(new Species.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new SpeciesLocation.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new Sighting.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserSpecies.EntityConfiguration());
    }
}

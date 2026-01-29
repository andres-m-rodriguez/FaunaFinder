using FaunaFinder.Wildlife.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Wildlife.Database;

public sealed class WildlifeDbContext(DbContextOptions<WildlifeDbContext> options)
    : DbContext(options)
{
    // Municipality data
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<MunicipalitySpecies> MunicipalitySpecies => Set<MunicipalitySpecies>();

    // Species data
    public DbSet<Species> Species => Set<Species>();
    public DbSet<SpeciesLocation> SpeciesLocations => Set<SpeciesLocation>();

    // Conservation data
    public DbSet<FwsAction> FwsActions => Set<FwsAction>();
    public DbSet<FwsLink> FwsLinks => Set<FwsLink>();
    public DbSet<NrcsPractice> NrcsPractices => Set<NrcsPractice>();

    // Wildlife Discovery
    public DbSet<Sighting> Sightings => Set<Sighting>();
    public DbSet<UserSpecies> UserSpecies => Set<UserSpecies>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from nested classes
        modelBuilder.ApplyConfiguration(new Municipality.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new MunicipalitySpecies.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new Species.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new SpeciesLocation.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new FwsAction.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new FwsLink.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new NrcsPractice.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new Sighting.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserSpecies.EntityConfiguration());
    }
}

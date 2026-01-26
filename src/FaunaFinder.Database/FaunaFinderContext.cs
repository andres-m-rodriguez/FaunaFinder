using FaunaFinder.Database.Models.Conservation;
using FaunaFinder.Database.Models.Municipalities;
using FaunaFinder.Database.Models.Species;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Database;

public sealed class FaunaFinderContext(DbContextOptions<FaunaFinderContext> options)
    : DbContext(options)
{
    // DbSets organized by domain
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<MunicipalitySpecies> MunicipalitySpecies => Set<MunicipalitySpecies>();
    public DbSet<Species> Species => Set<Species>();
    public DbSet<SpeciesLocation> SpeciesLocations => Set<SpeciesLocation>();
    public DbSet<SpeciesImage> SpeciesImages => Set<SpeciesImage>();
    public DbSet<FwsAction> FwsActions => Set<FwsAction>();
    public DbSet<FwsLink> FwsLinks => Set<FwsLink>();
    public DbSet<NrcsPractice> NrcsPractices => Set<NrcsPractice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from nested classes
        modelBuilder.ApplyConfiguration(new Municipality.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new MunicipalitySpecies.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new Species.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new SpeciesLocation.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new SpeciesImage.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new FwsAction.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new FwsLink.EntityConfiguration());
        modelBuilder.ApplyConfiguration(new NrcsPractice.EntityConfiguration());
    }
}

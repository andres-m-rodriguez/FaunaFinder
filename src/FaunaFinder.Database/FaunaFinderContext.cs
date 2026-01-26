using FaunaFinder.Database.Models.Conservation;
using FaunaFinder.Database.Models.Municipalities;
using FaunaFinder.Database.Models.Species;
using FaunaFinder.Database.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Database;

public sealed class FaunaFinderContext(DbContextOptions<FaunaFinderContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    // DbSets organized by domain
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<MunicipalitySpecies> MunicipalitySpecies => Set<MunicipalitySpecies>();
    public DbSet<Species> Species => Set<Species>();
    public DbSet<SpeciesLocation> SpeciesLocations => Set<SpeciesLocation>();
    public DbSet<FwsAction> FwsActions => Set<FwsAction>();
    public DbSet<FwsLink> FwsLinks => Set<FwsLink>();
    public DbSet<NrcsPractice> NrcsPractices => Set<NrcsPractice>();

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
        modelBuilder.ApplyConfiguration(new User.EntityConfiguration());

        // Configure Identity table names to use snake_case
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<IdentityRole<int>>().ToTable("roles");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("user_roles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("user_tokens");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("role_claims");
    }
}

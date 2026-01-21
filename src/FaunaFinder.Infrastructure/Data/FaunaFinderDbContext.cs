using FaunaFinder.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FaunaFinder.Infrastructure.Data;

public class FaunaFinderDbContext : DbContext
{
    public FaunaFinderDbContext(DbContextOptions<FaunaFinderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<Species> Species => Set<Species>();
    public DbSet<NrcsPractice> NrcsPractices => Set<NrcsPractice>();
    public DbSet<FwsAction> FwsActions => Set<FwsAction>();
    public DbSet<FwsLink> FwsLinks => Set<FwsLink>();
    public DbSet<MunicipalitySpecies> MunicipalitySpecies => Set<MunicipalitySpecies>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Municipality configuration
        modelBuilder.Entity<Municipality>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.GeoJsonId).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.GeoJsonId).IsUnique();
        });

        // Species configuration
        modelBuilder.Entity<Species>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CommonName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ScientificName).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.CommonName);
            entity.HasIndex(e => e.ScientificName);
        });

        // NrcsPractice configuration
        modelBuilder.Entity<NrcsPractice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // FwsAction configuration
        modelBuilder.Entity<FwsAction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // FwsLink configuration
        modelBuilder.Entity<FwsLink>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Justification).HasMaxLength(2000);

            entity.HasOne(e => e.NrcsPractice)
                .WithMany(p => p.FwsLinks)
                .HasForeignKey(e => e.NrcsPracticeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FwsAction)
                .WithMany(a => a.FwsLinks)
                .HasForeignKey(e => e.FwsActionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Species)
                .WithMany(s => s.FwsLinks)
                .HasForeignKey(e => e.SpeciesId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.NrcsPracticeId, e.FwsActionId, e.SpeciesId });
        });

        // MunicipalitySpecies configuration
        modelBuilder.Entity<MunicipalitySpecies>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Municipality)
                .WithMany(m => m.MunicipalitySpecies)
                .HasForeignKey(e => e.MunicipalityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Species)
                .WithMany(s => s.MunicipalitySpecies)
                .HasForeignKey(e => e.SpeciesId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.MunicipalityId, e.SpeciesId }).IsUnique();
        });
    }
}

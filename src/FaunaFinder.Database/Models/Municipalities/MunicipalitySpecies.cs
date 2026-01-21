using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Database.Models.Municipalities;

public sealed class MunicipalitySpecies
{
    public required int Id { get; set; }
    public required int MunicipalityId { get; set; }
    public required int SpeciesId { get; set; }

    public Municipality Municipality { get; set; } = null!;
    public Species.Species Species { get; set; } = null!;

    public sealed class EntityConfiguration : IEntityTypeConfiguration<MunicipalitySpecies>
    {
        public void Configure(EntityTypeBuilder<MunicipalitySpecies> builder)
        {
            builder.ToTable("municipality_species");
            builder.HasKey(static e => e.Id);

            builder.HasOne(static e => e.Municipality)
                .WithMany(static m => m.MunicipalitySpecies)
                .HasForeignKey(static e => e.MunicipalityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(static e => e.Species)
                .WithMany(static s => s.MunicipalitySpecies)
                .HasForeignKey(static e => e.SpeciesId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(static e => new { e.MunicipalityId, e.SpeciesId })
                .IsUnique()
                .HasDatabaseName("municipality_species_municipality_species_uidx");
        }
    }
}

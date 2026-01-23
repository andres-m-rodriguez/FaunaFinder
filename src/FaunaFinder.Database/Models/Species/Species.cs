using FaunaFinder.Database.Models.Conservation;
using FaunaFinder.Database.Models.Municipalities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Database.Models.Species;

public sealed class Species
{
    public required int Id { get; set; }
    public required string CommonName { get; set; }
    public required string ScientificName { get; set; }

    public ICollection<FwsLink> FwsLinks { get; set; } = [];
    public ICollection<MunicipalitySpecies> MunicipalitySpecies { get; set; } = [];
    public ICollection<SpeciesLocation> Locations { get; set; } = [];
    public ICollection<SpeciesTranslation> Translations { get; set; } = [];

    public sealed class EntityConfiguration : IEntityTypeConfiguration<Species>
    {
        public void Configure(EntityTypeBuilder<Species> builder)
        {
            builder.ToTable("species");
            builder.HasKey(static e => e.Id);

            builder.Property(static e => e.CommonName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(static e => e.ScientificName)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(static e => e.CommonName)
                .HasDatabaseName("species_common_name_idx");

            builder.HasIndex(static e => e.ScientificName)
                .IsUnique()
                .HasDatabaseName("species_scientific_name_uidx");
        }
    }
}

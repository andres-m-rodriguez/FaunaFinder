using FaunaFinder.i18n.Contracts;
using FaunaFinder.Database.Models.Conservation;
using FaunaFinder.Database.Models.Municipalities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Database.Models.Species;

public sealed class Species
{
    public required int Id { get; set; }
    public required List<LocaleValue> CommonName { get; set; }
    public required string ScientificName { get; set; }
    public required byte[]? ProfileImageData { get; set; }
    public required string? ProfileImageContentType { get; set; }

    public ICollection<FwsLink> FwsLinks { get; set; } = [];
    public ICollection<MunicipalitySpecies> MunicipalitySpecies { get; set; } = [];
    public ICollection<SpeciesLocation> Locations { get; set; } = [];

    public sealed class EntityConfiguration : IEntityTypeConfiguration<Species>
    {
        public void Configure(EntityTypeBuilder<Species> builder)
        {
            builder.ToTable("species");
            builder.HasKey(static e => e.Id);

            builder.OwnsMany(static e => e.CommonName, b => b.ToJson());

            builder.Property(static e => e.ScientificName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(static e => e.ProfileImageContentType)
                .HasMaxLength(100);

            builder.HasIndex(static e => e.ScientificName)
                .IsUnique()
                .HasDatabaseName("species_scientific_name_uidx");
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Database.Models.Species;

public sealed class SpeciesTranslation
{
    public required int Id { get; set; }
    public required int SpeciesId { get; set; }
    public required string LanguageCode { get; set; }
    public required string CommonName { get; set; }

    public Species Species { get; set; } = null!;

    public sealed class EntityConfiguration : IEntityTypeConfiguration<SpeciesTranslation>
    {
        public void Configure(EntityTypeBuilder<SpeciesTranslation> builder)
        {
            builder.ToTable("species_translations");
            builder.HasKey(static e => e.Id);

            builder.Property(static e => e.LanguageCode)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(static e => e.CommonName)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasOne(static e => e.Species)
                .WithMany(static s => s.Translations)
                .HasForeignKey(static e => e.SpeciesId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(static e => new { e.SpeciesId, e.LanguageCode })
                .IsUnique()
                .HasDatabaseName("species_translations_species_lang_uidx");
        }
    }
}

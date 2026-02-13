using FaunaFinder.i18n.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Wildlife.Database.Models;

public sealed class Species
{
    public required int Id { get; set; }
    public required List<LocaleValue> CommonName { get; set; }
    public required string ScientificName { get; set; }
    public required byte[]? ProfileImageData { get; set; }
    public required string? ProfileImageContentType { get; set; }
    public required string? ImageSourceUrl { get; set; }

    // Smart search fields
    public string? SearchText { get; set; }
    public List<string> SearchKeywords { get; set; } = [];

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

            builder.Property(static e => e.ScientificName).HasMaxLength(200).IsRequired();

            builder.Property(static e => e.ProfileImageContentType).HasMaxLength(100);

            builder.Property(static e => e.ImageSourceUrl).HasMaxLength(500);

            // Smart search fields
            builder.Property(static e => e.SearchText).HasMaxLength(1000);

            builder
                .HasIndex(static e => e.ScientificName)
                .IsUnique()
                .HasDatabaseName("species_scientific_name_uidx");

            // GIN index for trigram search on SearchText
            builder
                .HasIndex(static e => e.SearchText)
                .HasDatabaseName("species_search_text_gin_idx")
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");

            // GIN index for keyword array search
            builder
                .HasIndex(static e => e.SearchKeywords)
                .HasDatabaseName("species_search_keywords_gin_idx")
                .HasMethod("gin");
        }
    }
}

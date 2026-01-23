using FaunaFinder.Contracts.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Database.Models.Localization;

public sealed class Translation : ITranslation
{
    public required int Id { get; set; }
    public required int LanguageId { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }

    public Language Language { get; set; } = null!;

    // ITranslation implementation
    string ITranslation.LanguageCode => Language?.Code ?? string.Empty;

    public sealed class EntityConfiguration : IEntityTypeConfiguration<Translation>
    {
        public void Configure(EntityTypeBuilder<Translation> builder)
        {
            builder.ToTable("translations");
            builder.HasKey(static e => e.Id);

            builder.Property(static e => e.Key)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(static e => e.Value)
                .IsRequired();

            builder.HasOne(static e => e.Language)
                .WithMany(static e => e.Translations)
                .HasForeignKey(static e => e.LanguageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(static e => new { e.LanguageId, e.Key })
                .IsUnique()
                .HasDatabaseName("translations_language_key_uidx");

            builder.HasIndex(static e => e.Key)
                .HasDatabaseName("translations_key_idx");
        }
    }
}

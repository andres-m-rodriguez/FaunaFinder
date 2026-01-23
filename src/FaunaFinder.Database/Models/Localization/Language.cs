using FaunaFinder.Contracts.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Database.Models.Localization;

public sealed class Language : ILanguage
{
    public required int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required bool IsDefault { get; set; }

    public ICollection<Translation> Translations { get; set; } = [];

    public sealed class EntityConfiguration : IEntityTypeConfiguration<Language>
    {
        public void Configure(EntityTypeBuilder<Language> builder)
        {
            builder.ToTable("languages");
            builder.HasKey(static e => e.Id);

            builder.Property(static e => e.Code)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(static e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(static e => e.Code)
                .IsUnique()
                .HasDatabaseName("languages_code_uidx");
        }
    }
}

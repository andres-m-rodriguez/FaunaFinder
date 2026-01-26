using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Database.Models.Species;

public sealed class SpeciesImage
{
    public required int Id { get; set; }
    public required int SpeciesId { get; set; }
    public required byte[] ImageData { get; set; }
    public required string ContentType { get; set; }
    public string? FileName { get; set; }
    public string? Description { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Species Species { get; set; } = null!;

    public sealed class EntityConfiguration : IEntityTypeConfiguration<SpeciesImage>
    {
        public void Configure(EntityTypeBuilder<SpeciesImage> builder)
        {
            builder.ToTable("species_images");
            builder.HasKey(static e => e.Id);

            builder.Property(static e => e.ImageData)
                .IsRequired();

            builder.Property(static e => e.ContentType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(static e => e.FileName)
                .HasMaxLength(255);

            builder.Property(static e => e.Description)
                .HasMaxLength(500);

            builder.Property(static e => e.IsPrimary)
                .HasDefaultValue(false);

            builder.Property(static e => e.CreatedAt)
                .IsRequired();

            builder.HasOne(static e => e.Species)
                .WithMany(static e => e.Images)
                .HasForeignKey(static e => e.SpeciesId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(static e => e.SpeciesId)
                .HasDatabaseName("species_images_species_id_idx");

            // Index for finding primary images
            builder.HasIndex(static e => new { e.SpeciesId, e.IsPrimary })
                .HasDatabaseName("species_images_species_id_is_primary_idx");
        }
    }
}

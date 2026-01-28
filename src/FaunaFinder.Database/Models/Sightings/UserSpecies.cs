using FaunaFinder.Database.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Database.Models.Sightings;

/// <summary>
/// Represents a species submitted by a user that is not yet in the main species database.
/// These are flagged for expert verification before being added to the main Species table.
/// </summary>
public sealed class UserSpecies
{
    public required int Id { get; set; }
    public required string CommonName { get; set; }
    public required string? ScientificName { get; set; }
    public required string? Description { get; set; }
    public required byte[]? PhotoData { get; set; }
    public required string? PhotoContentType { get; set; }
    public required bool IsVerified { get; set; }
    public required bool IsEndangered { get; set; }
    public required int CreatedByUserId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime? VerifiedAt { get; set; }
    public required int? VerifiedByUserId { get; set; }
    public required int? ApprovedSpeciesId { get; set; }

    public User CreatedByUser { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
    public Species.Species? ApprovedSpecies { get; set; }
    public ICollection<Sighting> Sightings { get; set; } = [];

    public sealed class EntityConfiguration : IEntityTypeConfiguration<UserSpecies>
    {
        public void Configure(EntityTypeBuilder<UserSpecies> builder)
        {
            builder.ToTable("user_species");
            builder.HasKey(static e => e.Id);

            builder.Property(static e => e.CommonName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(static e => e.ScientificName)
                .HasMaxLength(200);

            builder.Property(static e => e.Description)
                .HasMaxLength(2000);

            builder.Property(static e => e.PhotoContentType)
                .HasMaxLength(100);

            builder.HasOne(static e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(static e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(static e => e.VerifiedByUser)
                .WithMany()
                .HasForeignKey(static e => e.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(static e => e.ApprovedSpecies)
                .WithMany()
                .HasForeignKey(static e => e.ApprovedSpeciesId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(static e => e.CommonName)
                .HasDatabaseName("user_species_common_name_idx");

            builder.HasIndex(static e => e.IsVerified)
                .HasDatabaseName("user_species_is_verified_idx");
        }
    }
}

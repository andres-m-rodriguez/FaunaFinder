using FaunaFinder.Database.Models.Species;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Database.Models.Conservation;

public sealed class FwsLink
{
    public required int Id { get; set; }
    public required int NrcsPracticeId { get; set; }
    public required int FwsActionId { get; set; }
    public required int SpeciesId { get; set; }
    public string? Justification { get; set; }

    public NrcsPractice NrcsPractice { get; set; } = null!;
    public FwsAction FwsAction { get; set; } = null!;
    public Species.Species Species { get; set; } = null!;

    public sealed class EntityConfiguration : IEntityTypeConfiguration<FwsLink>
    {
        public void Configure(EntityTypeBuilder<FwsLink> builder)
        {
            builder.ToTable("fws_links");
            builder.HasKey(static e => e.Id);

            builder.Property(static e => e.Justification)
                .HasMaxLength(2000);

            builder.HasOne(static e => e.NrcsPractice)
                .WithMany(static p => p.FwsLinks)
                .HasForeignKey(static e => e.NrcsPracticeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(static e => e.FwsAction)
                .WithMany(static a => a.FwsLinks)
                .HasForeignKey(static e => e.FwsActionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(static e => e.Species)
                .WithMany(static s => s.FwsLinks)
                .HasForeignKey(static e => e.SpeciesId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(static e => new { e.NrcsPracticeId, e.FwsActionId, e.SpeciesId })
                .HasDatabaseName("fws_links_practice_action_species_idx");
        }
    }
}

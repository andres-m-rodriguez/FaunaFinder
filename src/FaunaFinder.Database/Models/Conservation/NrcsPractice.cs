using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Database.Models.Conservation;

public sealed class NrcsPractice
{
    public required int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }

    public ICollection<FwsLink> FwsLinks { get; set; } = [];

    public sealed class EntityConfiguration : IEntityTypeConfiguration<NrcsPractice>
    {
        public void Configure(EntityTypeBuilder<NrcsPractice> builder)
        {
            builder.ToTable("nrcs_practices");
            builder.HasKey(static e => e.Id);

            builder.Property(static e => e.Code)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(static e => e.Name)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasIndex(static e => e.Code)
                .IsUnique()
                .HasDatabaseName("nrcs_practices_code_uidx");
        }
    }
}

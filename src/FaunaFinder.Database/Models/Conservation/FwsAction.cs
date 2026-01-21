using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Database.Models.Conservation;

public sealed class FwsAction
{
    public required int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }

    public ICollection<FwsLink> FwsLinks { get; set; } = [];

    public sealed class EntityConfiguration : IEntityTypeConfiguration<FwsAction>
    {
        public void Configure(EntityTypeBuilder<FwsAction> builder)
        {
            builder.ToTable("fws_actions");
            builder.HasKey(static e => e.Id);

            builder.Property(static e => e.Code)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(static e => e.Name)
                .HasMaxLength(500)
                .IsRequired();

            builder.HasIndex(static e => e.Code)
                .IsUnique()
                .HasDatabaseName("fws_actions_code_uidx");
        }
    }
}

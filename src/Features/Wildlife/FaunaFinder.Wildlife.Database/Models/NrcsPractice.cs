using FaunaFinder.i18n.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Wildlife.Database.Models;

public sealed class NrcsPractice
{
    public required int Id { get; set; }
    public required string Code { get; set; }
    public List<LocaleValue> Name { get; set; } = [];

    public ICollection<FwsLink> FwsLinks { get; set; } = [];

    public sealed class EntityConfiguration : IEntityTypeConfiguration<NrcsPractice>
    {
        public void Configure(EntityTypeBuilder<NrcsPractice> builder)
        {
            builder.ToTable("nrcs_practices");
            builder.HasKey(static e => e.Id);

            builder.Property(static e => e.Code).HasMaxLength(20).IsRequired();

            builder.OwnsMany(static e => e.Name, b => b.ToJson());

            builder
                .HasIndex(static e => e.Code)
                .IsUnique()
                .HasDatabaseName("nrcs_practices_code_uidx");
        }
    }
}

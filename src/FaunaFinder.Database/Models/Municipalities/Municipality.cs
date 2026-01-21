using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Database.Models.Municipalities;

public sealed class Municipality
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string GeoJsonId { get; set; }

    public ICollection<MunicipalitySpecies> MunicipalitySpecies { get; set; } = [];

    public sealed class EntityConfiguration : IEntityTypeConfiguration<Municipality>
    {
        public void Configure(EntityTypeBuilder<Municipality> builder)
        {
            builder.ToTable("municipalities");
            builder.HasKey(static e => e.Id);

            builder.Property(static e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(static e => e.GeoJsonId)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(static e => e.Name)
                .IsUnique()
                .HasDatabaseName("municipalities_name_uidx");

            builder.HasIndex(static e => e.GeoJsonId)
                .IsUnique()
                .HasDatabaseName("municipalities_geojson_id_uidx");
        }
    }
}

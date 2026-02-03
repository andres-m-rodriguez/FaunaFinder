using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Identity.Database.Models;

public sealed class AccessRequest
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string PasswordHash { get; set; }
    public string? Message { get; set; }
    public required AccessRequestStatus Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }

    public sealed class EntityConfiguration : IEntityTypeConfiguration<AccessRequest>
    {
        public void Configure(EntityTypeBuilder<AccessRequest> builder)
        {
            builder.ToTable("access_requests");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Email)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(e => e.DisplayName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.PasswordHash)
                .IsRequired();

            builder.Property(e => e.Message)
                .HasMaxLength(500);

            builder.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(e => e.Email);
            builder.HasIndex(e => e.Status);
        }
    }
}

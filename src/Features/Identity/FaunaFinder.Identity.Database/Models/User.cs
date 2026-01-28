using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FaunaFinder.Identity.Database.Models;

public sealed class User : IdentityUser<int>
{
    public required string DisplayName { get; set; }
    public required UserStatus Status { get; set; }
    public required UserRole Role { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }

    public sealed class EntityConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.Property(static e => e.DisplayName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(static e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(static e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Infrastructure.Persistence.Configurations;

public class PersonalAccessTokenConfiguration : IEntityTypeConfiguration<PersonalAccessToken>
{
    public void Configure(EntityTypeBuilder<PersonalAccessToken> builder)
    {
        builder.ToTable("personal_access_tokens");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(p => p.TokenHash)
            .IsUnique();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.IsRevoked)
            .HasDefaultValue(false);

        builder.HasOne(p => p.Subsection)
            .WithMany()
            .HasForeignKey(p => p.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Infrastructure.Persistence.Configurations;

public class PasswordResetCodeConfiguration : IEntityTypeConfiguration<PasswordResetCode>
{
    public void Configure(EntityTypeBuilder<PasswordResetCode> builder)
    {
        builder.ToTable("password_reset_codes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(6).IsFixedLength().IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.IsUsed).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => new { x.Email, x.IsUsed });
    }
}

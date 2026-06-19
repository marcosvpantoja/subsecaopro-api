using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Infrastructure.Persistence.Configurations;

public class SmtpConfigurationConfiguration : IEntityTypeConfiguration<SmtpConfiguration>
{
    public void Configure(EntityTypeBuilder<SmtpConfiguration> builder)
    {
        builder.ToTable("smtp_configurations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Host).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Port).IsRequired();
        builder.Property(x => x.Username).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Password).HasMaxLength(500).IsRequired();
        builder.Property(x => x.FromEmail).HasMaxLength(300).IsRequired();
        builder.Property(x => x.FromName).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.UpdatedAt);
    }
}

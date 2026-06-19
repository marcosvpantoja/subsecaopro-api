using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubseOpro.Domain;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Infrastructure.Persistence.Configurations;

public class PlatformAppSettingsConfiguration : IEntityTypeConfiguration<PlatformAppSettings>
{
    public void Configure(EntityTypeBuilder<PlatformAppSettings> builder)
    {
        builder.ToTable("platform_app_settings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PlatformName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.DefaultSubsectionSlug).HasMaxLength(80);
        builder.Property(x => x.AiMode).HasMaxLength(32).HasDefaultValue("mock");
        builder.Property(x => x.AiGlobalPrompt).HasMaxLength(4000);
        builder.Property(x => x.AiDisclaimer).HasMaxLength(1000);
        builder.Property(x => x.PublicModulesJson).HasColumnType("jsonb").IsRequired();
    }
}

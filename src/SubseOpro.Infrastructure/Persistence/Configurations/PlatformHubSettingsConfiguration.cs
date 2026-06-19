using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubseOpro.Domain;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Infrastructure.Persistence.Configurations;

public class PlatformHubSettingsConfiguration : IEntityTypeConfiguration<PlatformHubSettings>
{
    public static readonly Guid SingletonId = PlatformDefaults.HubSettingsId;

    public void Configure(EntityTypeBuilder<PlatformHubSettings> builder)
    {
        builder.ToTable("platform_hub_settings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.HubTitle).HasMaxLength(200).IsRequired();
        builder.Property(x => x.HubSubtitle).HasMaxLength(300).IsRequired();
        builder.Property(x => x.LoginBadge).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LoginHeadline).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LoginHeadlineAccent).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LoginDescription).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.Property(x => x.PrimaryColor).HasMaxLength(20).HasDefaultValue("#1B3A6B");
        builder.Property(x => x.AccentColor).HasMaxLength(20);
        builder.Property(x => x.FooterText).HasMaxLength(300).IsRequired();
    }
}

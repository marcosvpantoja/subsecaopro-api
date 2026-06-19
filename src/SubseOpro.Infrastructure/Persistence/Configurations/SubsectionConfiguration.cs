using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Enums;

namespace SubseOpro.Infrastructure.Persistence.Configurations;

public class SubsectionConfiguration : IEntityTypeConfiguration<Subsection>
{
    public void Configure(EntityTypeBuilder<Subsection> builder)
    {
        builder.ToTable("subsections");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Slug).HasMaxLength(80).IsRequired();
        builder.HasIndex(s => s.Slug).IsUnique();
        builder.Property(s => s.LogoUrl).HasMaxLength(500);
        builder.Property(s => s.PrimaryColor).HasMaxLength(20).HasDefaultValue("#1B3A6B");
        builder.Property(s => s.AccentColor).HasMaxLength(20);
        builder.Property(s => s.Description).HasMaxLength(2000);
        builder.Property(s => s.Address).HasMaxLength(300);
        builder.Property(s => s.Phone).HasMaxLength(30);
        builder.Property(s => s.Email).HasMaxLength(200);
        builder.Property(s => s.Hours).HasMaxLength(100);
        builder.Property(s => s.IsActive).HasDefaultValue(true);
        builder.Property(s => s.IaEnabled).HasDefaultValue(true);
        builder.Property(s => s.IaSystemPrompt).HasMaxLength(4000);
        builder.Property(s => s.IaDisclaimer).HasMaxLength(500);
        builder.Property(s => s.IaMaxTurns).HasDefaultValue(8);
        builder.Property(s => s.IaAreasCobertasJson).HasMaxLength(1000).HasDefaultValue("[]");
        builder.Property(s => s.HubAccessMode).HasConversion<string>().HasMaxLength(30).HasDefaultValue(HubAccessMode.Shared);
        builder.Property(s => s.MobileAccessMode).HasConversion<string>().HasMaxLength(30).HasDefaultValue(MobileAccessMode.SharedApp);
        builder.Property(s => s.DedicatedHubHost).HasMaxLength(300);
        builder.Property(s => s.EasProfileName).HasMaxLength(80);
        builder.Property(s => s.ActiveAiTokenEncrypted).HasMaxLength(512);
        builder.Property(s => s.AiServiceUrl).HasMaxLength(256);
    }
}

public class PlatformAdminConfiguration : IEntityTypeConfiguration<PlatformAdmin>
{
    public void Configure(EntityTypeBuilder<PlatformAdmin> builder)
    {
        builder.ToTable("platform_admins");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(200).IsRequired();
        builder.HasIndex(p => p.Email).IsUnique();
        builder.Property(p => p.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(p => p.IsActive).HasDefaultValue(true);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Enums;

namespace SubseOpro.Infrastructure.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Title).HasMaxLength(300);
        builder.Property(p => p.Caption).HasMaxLength(1000);
        builder.Property(p => p.ImageGradient).HasMaxLength(100);
        builder.Property(p => p.PhotoUrl).HasMaxLength(500);
        builder.Property(p => p.LinkUrl).HasMaxLength(500);
        builder.Property(p => p.Source)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(PostSource.Manual);
        builder.Property(p => p.IsActive).HasDefaultValue(true);
        builder.HasOne(p => p.Subsection)
            .WithMany()
            .HasForeignKey(p => p.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SocialAccountConfiguration : IEntityTypeConfiguration<SocialAccount>
{
    public void Configure(EntityTypeBuilder<SocialAccount> builder)
    {
        builder.ToTable("social_accounts");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Platform).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Handle).HasMaxLength(120).IsRequired();
        builder.Property(s => s.ProfileUrl).HasMaxLength(500).IsRequired();
        builder.HasOne(s => s.Subsection)
            .WithMany()
            .HasForeignKey(s => s.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("admin_users");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Email).HasMaxLength(200).IsRequired();
        builder.HasIndex(a => a.Email).IsUnique();
        builder.HasOne(a => a.Subsection)
            .WithMany()
            .HasForeignKey(a => a.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(a => a.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(a => a.Role).HasMaxLength(40).HasDefaultValue("moderator");
        builder.Property(a => a.IsActive).HasDefaultValue(true);
    }
}

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.ToTable("banners");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Title).HasMaxLength(200);
        builder.Property(b => b.LinkParams).HasColumnType("jsonb");
        builder.Property(b => b.PhotoUrl).HasMaxLength(500);
        builder.HasOne(b => b.Subsection)
            .WithMany()
            .HasForeignKey(b => b.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("partners");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Category).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Discount).HasMaxLength(20);
        builder.Property(p => p.Color).HasMaxLength(100);
        builder.Property(p => p.CouponCode).HasMaxLength(50).IsRequired();
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        builder.HasOne(p => p.Subsection)
            .WithMany()
            .HasForeignKey(p => p.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Text).HasMaxLength(500);
        builder.Property(n => n.Href).HasMaxLength(200);
        builder.HasOne(n => n.Subsection)
            .WithMany()
            .HasForeignKey(n => n.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(n => n.Reads)
            .WithOne(r => r.Notification)
            .HasForeignKey(r => r.NotificationId);
    }
}

public class NotificationReadConfiguration : IEntityTypeConfiguration<NotificationRead>
{
    public void Configure(EntityTypeBuilder<NotificationRead> builder)
    {
        builder.ToTable("notification_reads");
        builder.HasKey(r => new { r.NotificationId, r.UserId });
        builder.Property(r => r.ReadAt).IsRequired();
        builder.HasIndex(r => r.UserId);
    }
}

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("articles");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(a => new { a.SubsectionId, a.Slug }).IsUnique();
        builder.HasOne(a => a.Subsection)
            .WithMany()
            .HasForeignKey(a => a.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(a => a.Title).HasMaxLength(300);
    }
}

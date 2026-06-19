using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Infrastructure.Persistence.Configurations;

public class LawyerConfiguration : IEntityTypeConfiguration<Lawyer>
{
    public void Configure(EntityTypeBuilder<Lawyer> builder)
    {
        builder.ToTable("lawyers");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.OabNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(l => new { l.SubsectionId, l.OabNumber }).IsUnique();
        builder.HasOne(l => l.Subsection)
            .WithMany()
            .HasForeignKey(l => l.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(l => l.PasswordHash).HasMaxLength(256);
        builder.Property(l => l.Rating).HasPrecision(2, 1);
        builder.Property(l => l.Name).HasMaxLength(200).IsRequired();
        builder.Property(l => l.City).HasMaxLength(100);
        builder.Property(l => l.Email).HasMaxLength(200);
        builder.Property(l => l.Whatsapp).HasMaxLength(30);
        builder.Property(l => l.Initials).HasMaxLength(5);
        builder.Property(l => l.Color).HasMaxLength(100);
        builder.Property(l => l.PhotoUrl).HasMaxLength(500);
        builder.Property(l => l.CreatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

        builder.HasMany(l => l.LawyerSpecialties)
            .WithOne(ls => ls.Lawyer)
            .HasForeignKey(ls => ls.LawyerId);
    }
}

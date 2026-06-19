using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Infrastructure.Persistence.Configurations;

public class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
{
    public void Configure(EntityTypeBuilder<Specialty> builder)
    {
        builder.ToTable("specialties");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(s => new { s.SubsectionId, s.Name }).IsUnique();
        builder.HasOne(s => s.Subsection)
            .WithMany()
            .HasForeignKey(s => s.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class LawyerSpecialtyConfiguration : IEntityTypeConfiguration<LawyerSpecialty>
{
    public void Configure(EntityTypeBuilder<LawyerSpecialty> builder)
    {
        builder.ToTable("lawyer_specialties");
        builder.HasKey(ls => new { ls.LawyerId, ls.SpecialtyId });
    }
}

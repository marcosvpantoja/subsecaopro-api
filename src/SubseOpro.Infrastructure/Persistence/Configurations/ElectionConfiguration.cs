using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Infrastructure.Persistence.Configurations;

public class ElectionConfiguration : IEntityTypeConfiguration<Election>
{
    public void Configure(EntityTypeBuilder<Election> builder)
    {
        builder.ToTable("elections");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Term).HasMaxLength(30).IsRequired();
        builder.Property(e => e.PortalUrl).HasMaxLength(500);
        builder.HasOne(e => e.Subsection)
            .WithMany()
            .HasForeignKey(e => e.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.Candidates)
            .WithOne(c => c.Election)
            .HasForeignKey(c => c.ElectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("candidates");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Slate).HasMaxLength(200);
        builder.Property(c => c.Number).HasMaxLength(10);
        builder.Property(c => c.ColorStart).HasMaxLength(30);
        builder.Property(c => c.ColorEnd).HasMaxLength(30);
    }
}

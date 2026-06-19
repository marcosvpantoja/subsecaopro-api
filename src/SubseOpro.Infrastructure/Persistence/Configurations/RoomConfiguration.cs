using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Features).HasColumnType("jsonb");
        builder.Property(r => r.PricePerHour).HasPrecision(10, 2);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Color).HasMaxLength(100);
        builder.HasOne(r => r.Subsection)
            .WithMany()
            .HasForeignKey(r => r.SubsectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
